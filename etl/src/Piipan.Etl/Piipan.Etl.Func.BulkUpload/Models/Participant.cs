using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Utilities;

namespace Piipan.Etl.Func.BulkUpload.Models
{
    /// <summary>
    /// Represents a participant's CSV document row as object in the corresponding database structure
    /// </summary>
    public class Participant : IParticipant
    {
        public string LdsHash { get; set; } = null!;
        public string State { get; set; }
        public string CaseId { get; set; } = null!;
        public string ParticipantId { get; set; } = null!;
        public DateTime? ParticipantClosingDate { get; set; }
        public IEnumerable<DateRange> RecentBenefitIssuanceDates { get; set; } = new List<DateRange>();
        // Set Boolean values here, based on:
        // https://joshclose.github.io/CsvHelper/examples/configuration/attributes/
        // Values should mimic what is set in the Bulk Upload import schema
        [BooleanTrueValues("true")]
        [BooleanFalseValues("false")]
        public bool? VulnerableIndividual { get; set; }
    }
}