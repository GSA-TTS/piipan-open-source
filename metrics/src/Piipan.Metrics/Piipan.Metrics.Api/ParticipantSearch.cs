using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public class ParticipantSearch
    {
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("search_reason")]
        public string SearchReason { get; set; }
        [JsonProperty("search_from")]
        public string SearchFrom { get; set; }
        [JsonProperty("match_creation")]
        public string MatchCreation { get; set; }
        [JsonProperty("match_count")]
        public int MatchCount { get; set; }
        [JsonProperty("searched_at")]
        public DateTime SearchedAt { get; set; }
    }
}
