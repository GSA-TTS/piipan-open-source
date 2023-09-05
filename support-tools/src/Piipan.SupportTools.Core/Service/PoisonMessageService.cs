using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.SupportTools.Core.Models;

namespace Piipan.SupportTools.Core.Service
{
    /// <summary>
    /// Encapsulates all methods for PoisonMessageDequeuer azure function
    /// </summary>
    public class PoisonMessageService : IPoisonMessageService
    {
        private readonly ILogger<PoisonMessageService> _logger;
        public const int MaxMessasges = 20;
        public const int MaxLimitMessagesPerExecution = 10000;

        public PoisonMessageService(ILogger<PoisonMessageService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Dequeues messages from poison queue and sends them to regular queue
        /// </summary>
        /// <param name="targetQueue"></param>
        /// <param name="poisonQueue"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<int> RetryPoisonMessages(QueueClient targetQueue, QueueClient poisonQueue)
        {
            if (!targetQueue.Exists() || !poisonQueue.Exists())
            {
                throw new HttpRequestException("Target and/or Poison Queues do not exist. The Poison Queue must be named '{target queue name}-poison'");
            }

            var properties = poisonQueue.GetProperties();
            var cachedMessagesCountInPoisonQueue = properties.Value.ApproximateMessagesCount;
            _logger.LogInformation($"RetryPoisonMesssages: Number of messages in poison queue: {cachedMessagesCountInPoisonQueue}");

            //The maximum number of messages allowed per execution.
            cachedMessagesCountInPoisonQueue = cachedMessagesCountInPoisonQueue > MaxLimitMessagesPerExecution ? MaxLimitMessagesPerExecution : cachedMessagesCountInPoisonQueue;

            var countOfMessages = 0;

            //cachedMessagesCountInPoisonQueue is a circuit breaker
            //to avoid getting stuck in the while loop indefinitely
            while (cachedMessagesCountInPoisonQueue > 0)
            {
                // Receive and process 20 messages
                // It also sets the invisibility timeout to ten minutes for each message.
                // Note that the ten minutes starts for all messages at the same time, so after
                // ten minutes have passed since the call to ReceiveMessages, any messages which
                // have not been deleted will become visible again.

                QueueMessage[] messages = await poisonQueue.ReceiveMessagesAsync(MaxMessasges, TimeSpan.FromMinutes(10));

                if (messages == null || messages.Length == 0)
                    break;

                foreach (var message in messages)
                {
                    cachedMessagesCountInPoisonQueue--;

                    var sendMesResponse = await targetQueue.SendMessageAsync(message?.Body);
                    if (sendMesResponse?.Value == null)
                    {
                        _logger.LogError($"RetryPoisonMesssages: SendMessageAsync:: problem with sending the message. Message Id = {message?.MessageId}");
                        continue;
                    }

                    countOfMessages++;

                    var delMesResponse = await poisonQueue.DeleteMessageAsync(message?.MessageId, message?.PopReceipt);
                    if (delMesResponse?.IsError == true)
                    {
                        _logger.LogError($"RetryPoisonMesssages: DeleteMessageAsync:: error status {delMesResponse?.Status}. Message id {message?.MessageId}");
                    }
                }
            }

            if (countOfMessages == 0)
            {
                _logger.LogInformation($"There are no messages in poison queue");
                return countOfMessages;
            }

            _logger.LogInformation($"Received {countOfMessages} messages from poison queue and sent them to regular queue");

            return countOfMessages;
        }

        /// <summary>
        /// Deserializes json request body to PoisonMessageDequeuerParam object 
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        /// <exception cref="JsonSerializationException"></exception>
        public async Task<PoisonMessageDequeuerParam> Parse(Stream requestBody)
        {
            if (requestBody is null)
            {
                throw new JsonSerializationException("Request Body must not be null.");
            }

            var reader = new StreamReader(requestBody);
            var serialized = await reader.ReadToEndAsync();

            var param = JsonConvert.DeserializeObject<PoisonMessageDequeuerParam>(serialized);

            if (param is null)
            {
                throw new JsonSerializationException("Stream must not be empty.");
            }

            return param;
        }
    }
}