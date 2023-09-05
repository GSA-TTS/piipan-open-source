using System;
using System.ComponentModel.DataAnnotations;
using Piipan.Shared.API.Validation;

namespace Piipan.Metrics.Api
{
    public record ParticipantUploadRequestFilter
    {
        [Display(Name = "Start Date")]
        [UsaDate]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [UsaDate]
        [UsaMinimumDate(nameof(StartDate), ErrorMessage = "@@@ cannot be before the Start Date")]
        public DateTime? EndDate { get; set; }
        public int HoursOffset { get; set; }
        [Display(Name = "State")]
        public string State { get; set; }
        public string Status { get; set; }
        public int Page { get; set; } = 1;
        public int PerPage { get; set; } = 53;
    }
}
