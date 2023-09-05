using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.SupportTools.Core.Service;

namespace Piipan.SupportTools.Func.Api
{
    /// <summary>
    /// Dequeues poison messages in a message queue. 
    /// It contains the implementation details for processing and handling poison messages in a message-based system.
    /// </summary>
    public class PoisonMessageDequeuer
    {
        private readonly IPoisonMessageService _poisonMessageService;

        public PoisonMessageDequeuer(IPoisonMessageService poisonMessageService)
        {
            _poisonMessageService = poisonMessageService;
        }

        /// <summary>
        /// API endpoint for dequeue messages from poison queue
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="logger">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing manually
        /// </remarks>
        [FunctionName("PoisonMessageDequeuer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

                var param = await _poisonMessageService.Parse(req.Body);

                if (string.IsNullOrEmpty(param.QueueName)
                    || string.IsNullOrEmpty(param.AccountName)
                        || string.IsNullOrEmpty(param.AccountKey))
                {
                    log.LogInformation("The following information is necessary: Queue Name, Account Name, and Account Key");
                    return new BadRequestObjectResult("The following information is necessary: Queue Name, Account Name, and Account Key");
                }

                var storageAccountString = $"DefaultEndpointsProtocol=https;AccountName={param.AccountName};AccountKey={param.AccountKey}";

                var targetQueue = new QueueClient(storageAccountString, param.QueueName);
                var poisonQueue = new QueueClient(storageAccountString, param.QueueName + "-poison");

                var countOfMessages = await _poisonMessageService.RetryPoisonMessages(targetQueue, poisonQueue);

                return new OkObjectResult($"Received {countOfMessages} messages from {param.QueueName}-poison queue " +
                    $"and sent them to {param.QueueName} queue");
            }
            catch (HttpRequestException ex)
            {
                log.LogInformation("ERROR:: " + ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR:: " + ex.Message);
                return new InternalServerErrorResult();
            }
        }
    }
}
