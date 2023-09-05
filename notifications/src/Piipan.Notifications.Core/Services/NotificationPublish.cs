using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Services
{
    public class NotificationPublish : INotificationPublish
    {
        public EventGridPublisherClient _client;
        public ILogger<EmailModel> _logger;

        public NotificationPublish(ILogger<EmailModel> logger)
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
                    string eventGridUriString = Environment.GetEnvironmentVariable("EventGridNotifyEndPoint");
                    string eventGridKeyString = Environment.GetEnvironmentVariable("EventGridNotifyKeyString");

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
                        _logger.LogError("EventGridNotifyEndPoint and EventGridNotifyKeyString environment variables are not set.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to initialize EventGridPublisherClient.");
            }

        }

        /// <summary>
        /// Publishes an event to EventGrid containing Email queue
        /// </summary>
        /// <param name="emailModel">Bulk Upload Metadata And Metrics</param>
        /// <returns></returns>
        public Task PublishEmail(EmailModel emailModel)
        {
            try
            {
                var result = JsonConvert.SerializeObject(emailModel);
                //We want to use pre-serialized verion of the EmailModel.
                //Otherwise, EventGridEvent serializes it according to class property names rather than JsonProperty attributes
                var binaryData = BinaryData.FromString(result);
                // Add EventGridEvents to a list to publish to the topic
                EventGridEvent egEvent =
                    new EventGridEvent(
                        "Publish Notification",
                        "Publish to the queue",
                        "1.0",
                        binaryData);

                // Send the event
                _client.SendEventAsync(egEvent);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish emails to Notificationevent EventGrid.");
                throw;
            }
            return Task.CompletedTask;
        }

    }
}