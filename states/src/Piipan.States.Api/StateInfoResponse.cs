using Newtonsoft.Json;

namespace Piipan.States.Api
{
    public class StateInfoResponse
    {
        [JsonProperty("results")]
        public List<StateInfoResponseData> Results { get; set; } = new List<StateInfoResponseData>();
    }
}
