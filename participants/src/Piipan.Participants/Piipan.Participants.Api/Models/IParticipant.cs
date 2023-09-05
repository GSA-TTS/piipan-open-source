using Piipan.Shared.API.Utilities;
using System;
using System.Collections.Generic;

namespace Piipan.Participants.Api.Models
{
    public interface IParticipant
    {
        string LdsHash { get; set; }
        string State { get; set; }
        string CaseId { get; set; }
        string ParticipantId { get; set; }
        DateTime? ParticipantClosingDate { get; set; }
        IEnumerable<DateRange> RecentBenefitIssuanceDates { get; set; }
        bool? VulnerableIndividual { get; set; }
    }
}