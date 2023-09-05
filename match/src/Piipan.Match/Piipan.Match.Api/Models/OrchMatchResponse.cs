using Newtonsoft.Json;

namespace Piipan.Match.Api.Models
{
    /// <summary>
    /// Represents the top-level response body for a successful API response
    /// </summary>
    public class OrchMatchResponse
    {
        [JsonProperty("data")]
        public OrchMatchResponseData Data { get; set; } = new OrchMatchResponseData();
    }
}
