using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    /// <summary>
    /// implements getting latest upload from each state.
    /// </summary>
    public class GetUploadStatistics
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;

        public GetUploadStatistics(IParticipantUploadReaderApi participantUploadApi)
        {
            _participantUploadApi = participantUploadApi;
        }

        /// <summary>
        /// Azure Function implementing getting latest upload from each state.
        /// </summary>
        [FunctionName("GetUploadStatistics")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var query = req.Query; // this is IQueryCollection
                var json = JsonConvert.SerializeObject(query.ToDictionary(q => q.Key, q => q.Value.ToString()));
                var request = JsonConvert.DeserializeObject<ParticipantUploadStatisticsRequest>(json);

                var response = await _participantUploadApi.GetUploadStatistics(request);

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
