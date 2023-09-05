using System;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    public class ParticipantMatchMetrics
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("match_created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("init_state")]
        public string InitState { get; set; }

        [JsonProperty("init_state_invalid_match", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InitStateInvalidMatch { get; set; }

        [JsonProperty("init_state_invalid_match_reason")]
        public string? InitStateInvalidMatchReason { get; set; }

        [JsonProperty("init_state_initial_action_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? InitStateInitialActionAt { get; set; }

        [JsonProperty("init_state_initial_action_taken")]
        public string InitStateInitialActionTaken { get; set; }

        [JsonProperty("init_state_final_disposition", NullValueHandling = NullValueHandling.Ignore)]
        public string InitStateFinalDisposition { get; set; }

        [JsonProperty("init_state_final_disposition_date")]
        public DateTime? InitStateFinalDispositionDate { get; set; }

        [JsonProperty("init_state_vulnerable_individual", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InitStateVulnerableIndividual { get; set; }

        [JsonProperty("matching_state")]
        public string MatchingState { get; set; }

        [JsonProperty("matching_state_invalid_match", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MatchingStateInvalidMatch { get; set; }

        [JsonProperty("matching_state_invalid_match_reason")]
        public string? MatchingStateInvalidMatchReason { get; set; }

        [JsonProperty("matching_state_initial_action_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? MatchingStateInitialActionAt { get; set; }

        [JsonProperty("matching_state_initial_action_taken")]
        public string MatchingStateInitialActionTaken { get; set; }

        [JsonProperty("matching_state_final_disposition", NullValueHandling = NullValueHandling.Ignore)]
        public string MatchingStateFinalDisposition { get; set; }

        [JsonProperty("matching_state_final_disposition_date")]
        public DateTime? MatchingStateFinalDispositionDate { get; set; }

        [JsonProperty("matching_state_vulnerable_individual", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MatchingStateVulnerableIndividual { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}