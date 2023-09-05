using System;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

namespace Piipan.Match.Core.Services
{
    public class ParticipantPublishSearchMetric : IParticipantPublishSearchMetric
    {
        public EventGridPublisherClient _client;
        public ILogger<ParticipantPublishSearchMetric> _logger;

        public ParticipantPublishSearchMetric(ILogger<ParticipantPublishSearchMetric> logger)
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
                    string eventGridUriString = Environment.GetEnvironmentVariable("EventGridMetricSearchEndPoint");
                    string eventGridKeyString = Environment.GetEnvironmentVariable("EventGridMetricSearchKeyString");

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
                        _logger.LogError("EventGridMetricSearchEndPoint and EventGridMetricSearchKeyString environment variables are not set.");
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
        public Task PublishSearchMetrics(ParticipantSearchMetrics metrics)
        {
            try
            {
                foreach (ParticipantSearch metric in metrics.Data)
                {
                    var result = JsonConvert.SerializeObject(metric);
                    //We want to use pre-serialized verion of the ParticipantSearch.
                    //Otherwise, EventGridEvent serializes it according to class property names rather than JsonProperty attributes
                    var binaryData = BinaryData.FromString(result);
                    // Add EventGridEvents to a list to publish to the topic
                    EventGridEvent egEvent =
                        new EventGridEvent(
                            "Add participation",
                            "Upload to the database",
                            "1.0",
                            binaryData);

                    // Send the event
                    _client.SendEventAsync(egEvent);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Participant Search metrics event to EventGrid.");
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
