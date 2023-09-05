using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    // <summary>
    /// Metrics queue message for Uploads
    /// </summary>
    public class ParticipantUploadMetricsEvent
    {
        [JsonProperty("data", Required = Required.Always)]
        public ParticipantUpload Data { get; set; }
    }
}
