using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Api.Serializers;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Enums;
using Piipan.Shared.API.Utilities;

namespace Piipan.Match.Api.Models
{
    public class ParticipantMatch : IParticipant
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("lds_hash",
            NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public string LdsHash { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("case_id")]
        public string CaseId { get; set; }

        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }

        [JsonProperty("participant_closing_date")]
        [JsonConverter(typeof(JsonConverters.DateTimeConverter))]
        public DateTime? ParticipantClosingDate { get; set; }

        [JsonProperty("recent_benefit_issuance_dates")]
        [JsonConverter(typeof(JsonConverters.DateRangeConverter))]
        public IEnumerable<DateRange> RecentBenefitIssuanceDates { get; set; } = new List<DateRange>();

        [JsonProperty("vulnerable_individual")]
        public bool? VulnerableIndividual { get; set; }

        [JsonProperty("match_url")]
        public string MatchUrl { get; set; }
        [JsonProperty("match_creation")]
        [JsonIgnore]
        public string MatchCreation { get; set; } = EnumHelper.GetDisplayName(SearchMatchStatus.MATCHNOTFOUND);
        public ParticipantMatch() { }

        public ParticipantMatch(IParticipant p)
        {
            LdsHash = p.LdsHash;
            State = p.State;
            CaseId = p.CaseId;
            ParticipantId = p.ParticipantId;
            ParticipantClosingDate = p.ParticipantClosingDate;
            RecentBenefitIssuanceDates = p.RecentBenefitIssuanceDates;
            VulnerableIndividual = p.VulnerableIndividual;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
