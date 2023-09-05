using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    // <summary>
    /// Metrics queue message for Matches
    /// </summary>
    public class ParticipantMatchMetricsEvent
    {
        [JsonProperty("data", Required = Required.Always)]
        public ParticipantMatchMetrics Data { get; set; }
    }
}
