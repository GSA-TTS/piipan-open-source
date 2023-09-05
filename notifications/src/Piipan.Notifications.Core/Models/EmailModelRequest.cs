using Newtonsoft.Json;
using Piipan.Notifications.Models;

namespace Piipan.Notifications.Core.Models
{
    /// <summary>
    /// Full request body for Email Notification Request
    /// </summary>
    public class EmailModelRequest
    {
        [JsonProperty("data",
         Required = Required.Always)]
        public EmailModel Data { get; set; } = new EmailModel();
        public string Id { get; set; }
    }
}
