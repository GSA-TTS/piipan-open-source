using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    // <summary>
    /// Metrics queue message for Searches
    /// </summary>
    public class ParticipantSearchMetricsEvent
    {
        [JsonProperty("data", Required = Required.Always)]
        public ParticipantSearch Data { get; set; }
    }
}
