using System;
using System.Collections.Generic;
using Piipan.Match.Api.Serializers;
using Newtonsoft.Json;
using Piipan.Shared.API.Utilities;

namespace Piipan.Match.Api.Models.Resolution
{
    /// <summary>
    /// Initial participant data for each related state in a match
    /// <remarks>
    /// The duplicate properties are for the Participant data that is serialized from the match records
    /// which is in Pascal Case
    /// </remarks>
    /// </summary>
    public class Participant
    {
        [JsonProperty("case_id")]
        public string CaseId { get; set; }
        [JsonProperty("CaseId")]
        private string CaseId2 { set { CaseId = value; } }
        [JsonProperty("participant_closing_date")]
        [JsonConverter(typeof(JsonConverters.DateTimeConverter))]
        public DateTime? ParticipantClosingDate { get; set; }
        [JsonProperty("ParticipantClosingDate")]
        [JsonConverter(typeof(JsonConverters.DateTimeConverter))]
        private DateTime? ParticipantClosingDate2 { set { ParticipantClosingDate = value; } }
        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }
        [JsonProperty("ParticipantId")]
        private string ParticipantId2 { set { ParticipantId = value; } }
        [JsonProperty("recent_benefit_issuance_dates")]
        [JsonConverter(typeof(JsonConverters.DateRangeConverter))]
        // TODO: the data type of RecentBenefitIssuanceDates is still in flux
        // so DateTime[][] here is provisional
        public IEnumerable<DateRange> RecentBenefitIssuanceDates { get; set; } = new List<DateRange>();
        [JsonProperty("RecentBenefitIssuanceDates")]
        [JsonConverter(typeof(JsonConverters.DateRangeConverter))]
        private IEnumerable<DateRange> RecentBenefitIssuanceDates2 { set { RecentBenefitIssuanceDates = value; } }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("State")]
        private string State2 { set { State = value; } }
    }
}
