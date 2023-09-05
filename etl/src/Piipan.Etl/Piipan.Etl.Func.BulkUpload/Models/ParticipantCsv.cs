using CsvHelper.Configuration.Attributes;

namespace Piipan.Etl.Func.BulkUpload.Models
{
    /// <summary>
    /// Represents a participant's CSV document row to validate 
    /// </summary>
    public class ParticipantCsv
    {
        [Name("lds_hash")]
        public string LdsHash { get; set; } = null!;

        [Name("case_id")]
        public string CaseId { get; set; } = null!;

        [Name("participant_id")]
        public string ParticipantId { get; set; } = null!;

        [Name("participant_closing_date")]
        public string ParticipantClosingDate { get; set; }

        [Name("recent_benefit_issuance_dates")]
        public string RecentBenefitIssuanceDates { get; set; }

        [Name("vulnerable_individual")]
        public string VulnerableIndividual { get; set; }
    }
}