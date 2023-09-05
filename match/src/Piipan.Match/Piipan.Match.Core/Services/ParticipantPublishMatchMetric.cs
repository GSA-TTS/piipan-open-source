using System;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

namespace Piipan.Match.Core.Services
{
    public class ParticipantPublishMatchMetric : IParticipantPublishMatchMetric
    {
        public EventGridPublisherClient _client;
        public ILogger<ParticipantPublishMatchMetric> _logger;

        public ParticipantPublishMatchMetric(ILogger<ParticipantPublishMatchMetric> logger)
        {
            _logger = logger;
            InitializeEventGridPublisherClient();
        }

        private void InitializeEventGridPublisherClient()
        {
            try
            {
                if (_client == null)
                {
                    //Create event grid client to publish metric data
                    string eventGridUriString = Environment.GetEnvironmentVariable("EventGridMetricMatchEndPoint");
                    string eventGridKeyString = Environment.GetEnvironmentVariable("EventGridMetricMatchKeyString");

                    if (eventGridUriString != null && eventGridKeyString != null)
                    {
                        _client = new EventGridPublisherClient(
                            new Uri(eventGridUriString),
                            new AzureKeyCredential(eventGridKeyString),
                            default
                        );
                    }
                    else
                    {
                        _logger.LogError("EventGridMetricMatchEndPoint and EventGridMetricMatchKeyString environment variables are not set.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to initialize EventGridPublisherClient.");
            }

        }

        /// <summary>
        /// Publishes an event to EventGrid containing ParticipantUpload metrics
        /// </summary>
        /// <param name="metrics">Bulk Upload Metadata And Metrics</param>
        /// <returns></returns>
        public Task PublishMatchMetric(ParticipantMatchMetrics metrics)
        {
            try
            {
                var result = JsonConvert.SerializeObject(metrics);
                //We want to use pre-serialized verion of the ParticipantMatch.
                //Otherwise, EventGridEvent serializes it according to class property names rather than JsonProperty attributes
                var binaryData = BinaryData.FromString(result);
                // Add EventGridEvents to a list to publish to the topic
                EventGridEvent egEvent =
                    new EventGridEvent(
                        "Publish match metrics",
                        "Publish to the database",
                        "1.0",
                        binaryData);

                // Send the event
                _client.SendEventAsync(egEvent);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Publish Participant Match metrics event to EventGrid.");
                throw;
            }
            return Task.CompletedTask;
        }

    }
}