using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Notifications.Core.Models;
using Piipan.Notifications.Core.Services;

namespace Piipan.Notifications.Func.Api
{
    /// <summary>
    /// Azure Function implementing Email Notifications.
    /// </summary>
    public class NotificationApi
    {
        private readonly IMailService _mailService;
        public NotificationApi(IMailService mailService)
        {
            _mailService = mailService;
        }

        /// <summary>
        /// Function that processes email requests placed in Azure queue.
        /// </summary>
        /// <param name="emailRequest">The message retrieved from the queue for the function to process</param>
        /// <param name="log">handle to the function log</param>
        /// <returns></returns>
        [FunctionName("NotificationRequestProcessor")]
        public async Task Run([QueueTrigger("emailbucket", Connection = "")] string emailRequest, ILogger log)
        {
            try
            {
                if (emailRequest == null || emailRequest.Length == 0)
                {
                    log.LogError("No input was provided");
                }
                else
                {
                  
                    EmailModelRequest emailModelRequest = JsonConvert.DeserializeObject<EmailModelRequest>(emailRequest);
                    log.LogInformation($"Email Queue trigger function processed: Id: {emailModelRequest.Id} Email Subject: {emailModelRequest.Data.Subject} - Email Body: {emailModelRequest.Data.Body}");
                    bool mailSent = await _mailService.SendEmailAsync(emailModelRequest.Data, log, (ex) =>
                    {
                        log.LogError(ex.Message);
                    });
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
