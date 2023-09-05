using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Func.Collect
{
    public class PublishMatchMetrics
    {
        private readonly IParticipantMatchWriterApi _participantMatchWriterApi;

        public PublishMatchMetrics(IParticipantMatchWriterApi participantSearchWriterApi)
        {
            _participantMatchWriterApi = participantSearchWriterApi;
        }

        /// <summary>
        /// Listens for Find_Matches events when users searche participants;
        /// write meta info to Metrics database for any New Match
        /// </summary>
        /// <param name="metricsRequest">match resolution event metric event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("PublishMatchMetrics")]
        public async Task Run(
            [QueueTrigger("match-metrics", Connection = "")] ParticipantMatchMetricsEvent metricsRequest,
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
                ParticipantMatchMetrics newParticipantSearch = metricsRequest.Data;
                CheckParticipantMetrics(newParticipantSearch);
                int nRows = await _participantMatchWriterApi.PublishMatchMetrics(newParticipantSearch);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        private void CheckParticipantMetrics(ParticipantMatchMetrics metric)
        {
            if (metric.MatchId == null)
                throw new ArgumentException("Error with ParticipantMetrics");
        }
    }
}