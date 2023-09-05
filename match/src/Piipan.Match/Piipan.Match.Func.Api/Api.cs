using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Services;
using Piipan.Participants.Core;
using Piipan.Shared.Parsers;
using Piipan.Shared.Database;
using Piipan.Shared.Http;

namespace Piipan.Match.Func.Api
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class MatchApi
    {
        private readonly IMatchSearchApi _matchApi;
        private readonly IStreamParser<OrchMatchRequest> _requestParser;
        private readonly IMatchEventService _matchEventService;
        private readonly IMemoryCache _memoryCache;

        public MatchApi(
            IMatchSearchApi matchApi,
            IStreamParser<OrchMatchRequest> requestParser,
            IMatchEventService matchEventService,
            IMemoryCache memoryCache)
        {
            _matchApi = matchApi;
            _requestParser = requestParser;
            _matchEventService = matchEventService;
            _memoryCache = memoryCache;
            SqlMapper.AddTypeHandler(new DateRangeListHandler());
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>
        /// API endpoint for conducting matches across all participating states
        /// using de-identified data
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="logger">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with read
        /// access to the per-state participant databases.
        /// </remarks>
        [FunctionName("find_matches")]
        public async Task<IActionResult> Find(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            try
            {
                LogRequest(logger, req);

                var initiatingState = InitiatingState(req);
                var request = await _requestParser.Parse(req.Body);
                string apimSubscriptionName = req.Headers["Ocp-Apim-Subscription-Name"];
                string searchFrom = !string.IsNullOrEmpty(apimSubscriptionName) ? "API" : "Web Application";

                // If our initiating state is in the enabled states list return the result, otherwise return nothing.
                var enabledStatesList = _memoryCache.GetOrCreate("EnabledStates", (entry) =>
                {
                    string enabledStates = Environment.GetEnvironmentVariable("EnabledStates")?.ToLower();
                    return enabledStates?.Split(',') ?? new string[0];
                });

                var response = await _matchApi.FindAllMatches(request.ParsedRequest, initiatingState);
                response = await _matchEventService.ResolveMatches(request.ParsedRequest, response, initiatingState, searchFrom,enabledStatesList);

                // If our initiating state is not enabled, just return empty.
                if (!enabledStatesList.Contains(initiatingState.ToLower()))
                {
                    return EmptyMatchResponse;
                }

                if (response?.Data?.Results?.Count > 0)
                {
                    foreach (var result in response.Data.Results)
                    {
                        result.Matches = result.Matches?.Where(match => match.State != null && enabledStatesList.Contains(match.State.ToLower())).ToList();
                    }
                }

                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
            }
            catch (StreamParserException ex)
            {
                return DeserializationErrorResponse(ex);
            }
            catch (ValidationException ex)
            {
                return ValidationErrorResponse(ex);
            }
            catch (HttpRequestException ex)
            {
                return BadRequestErrorresponse(ex);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        private JsonResult EmptyMatchResponse => new JsonResult(
            new OrchMatchResponse()
            {
                Data = new OrchMatchResponseData()
                {
                    Results = new System.Collections.Generic.List<OrchMatchResult>
                    {
                        new OrchMatchResult()
                        {
                            Matches = Enumerable.Empty<ParticipantMatch>()
                        }
                    }
                }
            })
        { StatusCode = StatusCodes.Status200OK };

        private void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string subscription = request.Headers["Ocp-Apim-Subscription-Name"];
            if (subscription != null)
            {
                logger.LogInformation("Using APIM subscription {Subscription}", subscription);
            }
            else
            {
                logger.LogInformation("No APIM Subscription found. Requested from Web App.");
            }

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
        }

        private string InitiatingState(HttpRequest request)
        {
            string state = request.Headers["X-Initiating-State"];

            if (String.IsNullOrEmpty(state))
            {
                throw new HttpRequestException("Request is missing required header: X-Initiating-State");
            }
            return state.ToLower();
        }

        private ActionResult ValidationErrorResponse(ValidationException exception)
        {
            var errResponse = new ApiErrorResponse();
            foreach (var failure in exception.Errors)
            {
                errResponse.Errors.Add(new ApiHttpError()
                {
                    Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                    Title = failure.ErrorCode,
                    Detail = failure.ErrorMessage
                });
            }
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        private ActionResult DeserializationErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = Convert.ToString(ex.GetType()),
                Detail = ex.Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        private ActionResult InternalServerErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.InternalServerError),
                Title = ex.GetType().Name,
                Detail = ex.Message
            });
            return (ActionResult)new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        private ActionResult BadRequestErrorresponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = "Bad request",
                Detail = ex.Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }
    }
}
