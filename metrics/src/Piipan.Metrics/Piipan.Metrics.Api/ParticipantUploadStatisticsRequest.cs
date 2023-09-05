using System;

namespace Piipan.Metrics.Api
{
    public record ParticipantUploadStatisticsRequest
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public int HoursOffset { get; set; }
    }
}
