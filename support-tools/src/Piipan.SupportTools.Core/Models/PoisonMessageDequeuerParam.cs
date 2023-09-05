using Newtonsoft.Json;

namespace Piipan.SupportTools.Core.Models
{
    /// <summary>
    /// Represents the body json from PoisonMessageDequeuer azure function
    /// </summary>
    public class PoisonMessageDequeuerParam
    {
        [JsonProperty("queue_name")]
        public string QueueName { get; set; } = string.Empty;
        [JsonProperty("account_name")]
        public string AccountName { get; set; } = string.Empty;
        [JsonProperty("account_key")]
        public string AccountKey { get; set; } = string.Empty;
    }
}
