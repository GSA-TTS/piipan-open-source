using Azure.Storage.Queues;
using Piipan.SupportTools.Core.Models;

namespace Piipan.SupportTools.Core.Service
{
    /// <summary>
    /// Defines two methods for processing poison messages in a message queue.
    /// </summary>
    public interface IPoisonMessageService
    {
        Task<int> RetryPoisonMessages(QueueClient targetQueue, QueueClient poisonQueue);
        Task<PoisonMessageDequeuerParam> Parse(Stream requestBody);
    }
}
