using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    /// <summary>
    /// implements getting latest upload from each state.
    /// </summary>
    public class GetLastUpload
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;

        public GetLastUpload(IParticipantUploadReaderApi participantUploadApi)
        {
            _participantUploadApi = participantUploadApi;
        }

        /// <summary>
        /// Azure Function implementing getting latest upload from each state.
        /// </summary>
        [FunctionName("GetLastUpload")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var response = await _participantUploadApi.GetLatestUploadsByState();

                return (ActionResult)new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
