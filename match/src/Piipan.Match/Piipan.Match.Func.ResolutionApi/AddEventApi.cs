using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Metrics.Api;
using Piipan.Shared.Parsers;
using Piipan.Shared.Http;

#nullable enable

namespace Piipan.Match.Func.ResolutionApi
{
    /// <summary>
    /// Azure Function implementing Disposition Update endpoint for Match Resolution API
    /// </summary>
    public class AddEventApi : BaseApi
    {
        private readonly IMatchDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;
        private readonly IMatchDetailsAggregator _matchResAggregator;
        private readonly IStreamParser<AddEventRequest> _requestParser;
        private readonly IParticipantPublishMatchMetric _participantPublishMatchMetric;
        public readonly string UserActor = "user";
        public readonly string SystemActor = "system";
        public readonly string ClosedDelta = "{\"status\": \"closed\"}";

        public AddEventApi(
            IMatchDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchDetailsAggregator matchResAggregator,
            IStreamParser<AddEventRequest> requestParser,
            IParticipantPublishMatchMetric participantPublishMatchMetric)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _requestParser = requestParser;
            _participantPublishMatchMetric = participantPublishMatchMetric;
        }

        [FunctionName("AddEvent")]
        public async Task<IActionResult> AddEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "matches/{matchId}/disposition")] HttpRequest req,
            string matchId,
            ILogger logger)
        {
            LogRequest(logger, req);

            try
            {
                matchId = matchId?.ToUpper();
                var (reqObj, rawRequest) = await _requestParser.Parse(req.Body);
                var match = _matchRecordDao.GetRecordByMatchId(matchId);

                var matchResEvents = _matchResEventDao.GetEventsByMatchId(matchId);
                await Task.WhenAll(match, matchResEvents);
                // If state does not belong to match, return unauthorized
                string state = req.Headers["X-Initiating-State"];
                state = state.ToLower();
                if (!match.Result.States.Contains(state))
                {
                    return UnauthorizedErrorResponse();
                }
                // If match is closed, return unauthorized
                var matchResRecord = _matchResAggregator.BuildAggregateMatchDetails(match.Result, matchResEvents.Result);
                if (matchResRecord.Status == MatchRecordStatus.Closed)
                {
                    return UnauthorizedErrorResponse();
                }

                // Additional validation here that couldn't be done in the AddEventRequestValidator since we didn't have a match object to compare against
                if (reqObj.Data.FinalDispositionDate != null && reqObj.Data.FinalDispositionDate.Value < match.Result.CreatedAt?.Date)
                {
                    string errorPrefix = reqObj.Data.FinalDisposition switch
                    {
                        "Benefits Approved" => "Benefits Start Date",
                        "Benefits Terminated" => "Benefits End Date",
                        _ => "Final Disposition Date"
                    };
                    throw new ValidationException("request validation failed",
                        new ValidationFailure[] {
                            new ValidationFailure(nameof(reqObj.Data.FinalDispositionDate), $"{errorPrefix} cannot be before the match date of {match.Result.CreatedAt.Value.ToString("MM/dd/yyyy")}")
                        });
                }

                // insert event
                var newEvent = new MatchResEventDbo()
                {
                    MatchId = matchId,
                    ActorState = state,
                    Actor = req.Headers["From"].ToString() ?? UserActor,
                    Delta = rawRequest["data"]?.ToString(),
                    InsertedAt = DateTime.MaxValue // Note, this is only for sorting purposes during BuildAggregateMatchDetails. This does not get saved to the database.
                };
                var updatedMatchResEvents = matchResEvents.Result.ToList();
                updatedMatchResEvents.Add(newEvent);
                var matchBeforeClosureDetermination = _matchResAggregator.BuildAggregateMatchDetails(match.Result, updatedMatchResEvents);

                // If saving this would cause no changes, throw an error.
                IMatchResEvent? lastEvent = matchResEvents.Result.LastOrDefault();
                if (JsonConvert.SerializeObject(matchBeforeClosureDetermination) == JsonConvert.SerializeObject(matchResRecord))
                {
                    string message = "Duplicate action not allowed";
                    logger.LogError(message);
                    return UnprocessableEntityResponse(message);
                }

                var successfulEventAdd = await _matchResEventDao.AddEvent(newEvent);
                if (successfulEventAdd == 0)
                {
                    string message = "Match update save failed. Please try again later.";
                    logger.LogError(message);
                    return UnprocessableEntityResponse(message);
                }
                // determine if match should be closed

                await DetermineClosure(matchBeforeClosureDetermination);
                // Update the latest record to the Metrics database.
                var matchResEventsAfterUpdate = _matchResEventDao.GetEventsByMatchId(matchId);
                await Task.WhenAll(match, matchResEventsAfterUpdate);
                var matchResRecordAfterUpdate = _matchResAggregator.BuildAggregateMatchDetails(match.Result, matchResEventsAfterUpdate.Result);

                await PublishMatchMetrics(matchResRecordAfterUpdate);

                return new OkResult();
            }
            catch (StreamParserException ex)
            {
                logger.LogError(ex.Message);
                return DeserializationErrorResponse(ex);
            }
            catch (ValidationException ex)
            {
                logger.LogError(ex.Message);
                return ValidationErrorResponse(ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex.Message);
                return NotFoundErrorResponse(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }

        private async Task PublishMatchMetrics(MatchDetailsDto matchResRecordAfterUpdate)
        {
            //BuildAggregateMatchDetails Search Metrics
            var participantMatchMetrics = new ParticipantMatchMetrics()
            {
                MatchId = matchResRecordAfterUpdate.MatchId,
                InitState = matchResRecordAfterUpdate.Initiator,
                MatchingState = matchResRecordAfterUpdate.States[1],
                CreatedAt = matchResRecordAfterUpdate.CreatedAt,
                Status = matchResRecordAfterUpdate.Status,
                InitStateInvalidMatch = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatch).FirstOrDefault(),
                InitStateInvalidMatchReason = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatchReason).FirstOrDefault(),
                InitStateInitialActionAt = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionAt).FirstOrDefault(),
                InitStateInitialActionTaken = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionTaken).FirstOrDefault(),
                InitStateFinalDisposition = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDisposition).FirstOrDefault(),
                InitStateFinalDispositionDate = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDispositionDate).FirstOrDefault(),
                InitStateVulnerableIndividual = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.VulnerableIndividual).FirstOrDefault(),
                MatchingStateInvalidMatch = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatch).FirstOrDefault(),
                MatchingStateInvalidMatchReason = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatchReason).FirstOrDefault(),
                MatchingStateInitialActionAt = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionAt).FirstOrDefault(),
                MatchingStateInitialActionTaken = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionTaken).FirstOrDefault(),
                MatchingStateFinalDisposition = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDisposition).FirstOrDefault(),
                MatchingStateFinalDispositionDate = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDispositionDate).FirstOrDefault(),
                MatchingStateVulnerableIndividual = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.VulnerableIndividual).FirstOrDefault(),
            };
            await _participantPublishMatchMetric.PublishMatchMetric(participantMatchMetrics);
        }



        private async Task DetermineClosure(MatchDetailsDto matchBeforeClosureDetermination)
        {
            if (matchBeforeClosureDetermination.Dispositions.All(n => IsDispositionReadyToClose(n)))
            {
                var closedEvent = new MatchResEventDbo()
                {
                    MatchId = matchBeforeClosureDetermination.MatchId,
                    Actor = SystemActor,
                    Delta = ClosedDelta
                };
                await _matchResEventDao.AddEvent(closedEvent);
            }
        }

        private bool IsDispositionReadyToClose(Disposition disposition)
        {
            return (!string.IsNullOrEmpty(disposition?.FinalDisposition) && disposition?.FinalDispositionDate != null) || disposition?.InvalidMatch == true;
        }

        private ActionResult UnprocessableEntityResponse(string message)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.UnprocessableEntity),
                Title = "UnprocessableEntity",
                Detail = message
            });
            return (ActionResult)new UnprocessableEntityObjectResult(errResponse);
        }

        private ActionResult UnauthorizedErrorResponse()
        {
            return (ActionResult)new UnauthorizedResult();
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

    }
}