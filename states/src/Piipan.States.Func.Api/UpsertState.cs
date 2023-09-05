using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Parsers;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;

namespace Piipan.States.Func.Api
{
    /// <summary>
    /// Azure endpoint adding updating and inserting state info functionality
    /// </summary>
    public class UpsertState
    {
        private readonly IStateInfoDao _stateInfoDao;
        private readonly IStreamParser<StateInfoRequest> _requestParser;
        private readonly ICryptographyClient _cryptographyClient;

        public UpsertState(IStateInfoDao stateInfoDao, IStreamParser<StateInfoRequest> requestParser, ICryptographyClient cryptographyClient)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            _stateInfoDao = stateInfoDao;
            _requestParser = requestParser;
            _cryptographyClient = cryptographyClient;
        }

        /// <summary>
        /// API endpoint for adding or inserting state info
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="logger">handle to the function log</param>
        [FunctionName("upsert_state")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = null)] HttpRequest req,
            ILogger logger)
        {
            try
            {
                LogRequest(logger, req);
                var request = await _requestParser.Parse(req.Body);
                StateInfoRequest insertStateRequest = request.ParsedRequest;
                StateInfoDto insertStateInfo = insertStateRequest.Data;

                string encryptedEmail = _cryptographyClient.EncryptToBase64String(insertStateInfo.Email);
                string encryptedPhone = _cryptographyClient.EncryptToBase64String(insertStateInfo.Phone);
                string encryptedEmail_CC = _cryptographyClient.EncryptToBase64String(insertStateInfo.EmailCc);
                var successfulUpsert = await _stateInfoDao.UpsertState(new StateInfoDto
                {
                    Id = insertStateInfo.Id,
                    Email = encryptedEmail,
                    Phone = encryptedPhone,
                    EmailCc = encryptedEmail_CC,
                    Region = insertStateInfo.Region,
                    State = insertStateInfo.State,
                    StateAbbreviation = insertStateInfo.StateAbbreviation
                });

                if (successfulUpsert == 1)
                {
                    return new JsonResult("Successfully Upserted")
                    {
                        StatusCode = StatusCodes.Status200OK
                    };
                }
                else
                {
                    return new JsonResult("Failed Upsert")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
                
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
