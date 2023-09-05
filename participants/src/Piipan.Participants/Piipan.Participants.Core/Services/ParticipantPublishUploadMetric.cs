using System;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;

namespace Piipan.Participants.Core.Services
{
    /// <summary>
    /// Class responsible for publishing an EventGridEvent for status updates regarding BulkUpload metrics
    /// </summary>
    public class ParticipantPublishUploadMetric : IParticipantPublishUploadMetric
    {
        public EventGridPublisherClient _client;
        public ILogger<ParticipantPublishUploadMetric> _logger;

        public ParticipantPublishUploadMetric(ILogger<ParticipantPublishUploadMetric> logger)
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
                    string eventGridUriString = Environment.GetEnvironmentVariable("EventGridEndPoint");
                    string eventGridKeyString = Environment.GetEnvironmentVariable("EventGridKeyString");

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
                        _logger.LogWarning("EventGridEndPoint and EventGridKeyString environment variables are not set.");
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
        public Task PublishUploadMetric(ParticipantUpload metrics)
        {
            try
            {
                var result = JsonConvert.SerializeObject(metrics);

                //We want to use pre-serialized verion of the ParticipantUpload.
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

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish ParticipantUpload metrics event to EventGrid.");
                throw;
            }
        }
    }
}