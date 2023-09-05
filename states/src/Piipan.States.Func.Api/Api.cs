using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Piipan.States.Core.Service;

namespace Piipan.States.Func.Api
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class StateApi
    {
        private readonly IStateInfoService _stateInfoService;

        public StateApi(IStateInfoService stateInfoService)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            _stateInfoService = stateInfoService;
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
        [FunctionName("states")]
        public async Task<IActionResult> GetStates(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger logger)
        {
            try
            {
                LogRequest(logger, req);

                var states = await _stateInfoService.GetDecryptedStates();

                return new JsonResult(new StatesInfoResponse
                {
                    Results = states.Select(s => s.CopyTo<StateInfoDto>())
                })
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        private void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
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
            return new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
