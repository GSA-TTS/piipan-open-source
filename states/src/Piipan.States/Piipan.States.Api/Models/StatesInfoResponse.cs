using Newtonsoft.Json;

namespace Piipan.States.Api.Models
{
    public class StatesInfoResponse
    {
        [JsonProperty("data")]
        public IEnumerable<StateInfoDto> Results { get; set; } = Enumerable.Empty<StateInfoDto>();
    }
}
