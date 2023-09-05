using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Match.Api.Models
{

    /// <summary>
    /// Represents the entire result for each person from an API request
    /// <para> Collects all matches from all states.</para>
    /// <para> Adds more properties than the generic responses from the per-state API's.</para>
    /// </summary>
    public class OrchMatchResult
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("matches")]
        public IEnumerable<ParticipantMatch> Matches { get; set; }
    }
}
