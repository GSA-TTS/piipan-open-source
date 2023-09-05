using Newtonsoft.Json;
using System;

namespace Piipan.Match.Api.Models.Resolution
{
    /// <summary>
    /// Aggregate match resolution data that a MatchResAggregator returns
    /// </summary>
    public class MatchDetailsDto
    {
        [JsonProperty("dispositions")]
        public Disposition[] Dispositions { get; set; } = Array.Empty<Disposition>();
        [JsonProperty("initiator")]
        public string Initiator { get; set; }
        [JsonProperty("match_id")]
        public string MatchId { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty("participants")]
        public Participant[] Participants { get; set; } = Array.Empty<Participant>();
        [JsonProperty("states")]
        public string[] States { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; } = MatchRecordStatus.Open;
    }
}
