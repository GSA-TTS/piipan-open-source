using System;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.Models
{
    /// <summary>
    /// Implementation of ParticipantSearch record for Metrics database interactions
    /// </summary>
    public class ParticipantMatchDbo
    {
        public string MatchId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string InitState { get; set; }

        public bool? InitStateInvalidMatch { get; set; }

        public string? InitStateInvalidMatchReason { get; set; }

        public DateTime? InitStateInitialActionAt { get; set; }

        public string InitStateInitialActionTaken { get; set; }

        public string InitStateFinalDisposition { get; set; }

        public DateTime? InitStateFinalDispositionDate { get; set; }

        public bool? InitStateVulnerableIndividual { get; set; }

        public string MatchingState { get; set; }

        public bool? MatchingStateInvalidMatch { get; set; }

        public string? MatchingStateInvalidMatchReason { get; set; }

        public DateTime? MatchingStateInitialActionAt { get; set; }

        public string MatchingStateInitialActionTaken { get; set; }

        public string MatchingStateFinalDisposition { get; set; }

        public DateTime? MatchingStateFinalDispositionDate { get; set; }

        public bool? MatchingStateVulnerableIndividual { get; set; }

        public string Status { get; set; }//= MatchRecordStatus.Open;

        public ParticipantMatchDbo()
        {
        }

        public ParticipantMatchDbo(ParticipantMatchMetrics match)
        {
            MatchId = match.MatchId;
            CreatedAt = match.CreatedAt;
            InitState = match.InitState;
            InitStateInvalidMatch = match.InitStateInvalidMatch;
            InitStateInvalidMatchReason = match.InitStateInvalidMatchReason;
            InitStateInitialActionAt = match.InitStateInitialActionAt;
            InitStateInitialActionTaken = match.InitStateInitialActionTaken;
            InitStateFinalDisposition = match.InitStateFinalDisposition;
            InitStateFinalDispositionDate = match.InitStateFinalDispositionDate;
            InitStateVulnerableIndividual = match.InitStateVulnerableIndividual;
            MatchingState = match.MatchingState;
            MatchingStateInvalidMatch = match.MatchingStateInvalidMatch;
            MatchingStateInvalidMatchReason = match.MatchingStateInvalidMatchReason;
            MatchingStateInitialActionAt = match.MatchingStateInitialActionAt;
            MatchingStateInitialActionTaken = match.MatchingStateInitialActionTaken;
            MatchingStateFinalDisposition = match.MatchingStateFinalDisposition;
            MatchingStateFinalDispositionDate = match.MatchingStateFinalDispositionDate;
            MatchingStateVulnerableIndividual = match.MatchingStateVulnerableIndividual;
            Status = match.Status;
        }
    }
}