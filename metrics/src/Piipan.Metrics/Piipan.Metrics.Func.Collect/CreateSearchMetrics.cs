using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;

namespace Piipan.Metrics.Func.Collect
{
    public class CreateSearchMetrics
    {
        private readonly IParticipantSearchWriterApi _participantSearchWriterApi;

        public CreateSearchMetrics(IParticipantSearchWriterApi participantSearchWriterApi)
        {
            _participantSearchWriterApi = participantSearchWriterApi;
        }
        /// <summary>
        /// Listens for Find_Matches events when users searche participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="metricsRequest">duplicate participation search metric event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("CreateSearchMetrics")]
        public async Task Run(
            [QueueTrigger("search-metrics", Connection = "")] ParticipantSearchMetricsEvent metricsRequest,
            ILogger log)
        {
            if (metricsRequest == null)
            {
                log.LogError("No input was provided");
                throw new ArgumentNullException(nameof(metricsRequest));
            }

            log.LogInformation(metricsRequest.Data.ToString());
            try
            {
                ParticipantSearch newParticipantSearch = metricsRequest.Data;
                CheckParticipantSearch(newParticipantSearch);
                int nRows = await _participantSearchWriterApi.AddSearchMetrics(newParticipantSearch);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
        private void CheckParticipantSearch(ParticipantSearch metric)
        {
            if (metric.State == null)
                throw new ArgumentException("Error with ParticipantSearch");
        }
    }
}
