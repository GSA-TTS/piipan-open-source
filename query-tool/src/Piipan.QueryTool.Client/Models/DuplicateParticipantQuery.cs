using System;
using System.ComponentModel.DataAnnotations;
using Piipan.Shared.API.Validation;
using static Piipan.Shared.API.Validation.ValidationConstants;
namespace Piipan.QueryTool.Client.Models
{
    /// <summary>
    /// Represents form input from user for a match query
    /// </summary>
    public class DuplicateParticipantQuery
    {
        [UsaRequired]
        [UsaName]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [UsaRequired]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date),
            DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [UsaDate(MaxValue: UsaDateAttribute.Today)]
        public DateTime? DateOfBirth { get; set; }

        [UsaRequired]
        [UsaSSN]
        [Display(Name = "Social Security Number")]
        public string SocialSecurityNum { get; set; }

        [UsaRequired]
        [Display(Name = "Participant ID")]
        [RegularExpression("^[A-Za-z0-9-_]+$", ErrorMessage = $"{ValidationFieldPlaceholder} must contain uppercase letters (A-Z), lowercase letters (a-z), numbers (0-9), underscore (_), dash (-).")]
        [MaxLength(20, ErrorMessage = $"{ValidationFieldPlaceholder} can be maximum 20 characters.")]
        public string ParticipantId { get; set; }

        [Display(Name = "Case Number")]
        [RegularExpression("^[A-Za-z0-9-_]+$", ErrorMessage = $"{ValidationFieldPlaceholder} must contain uppercase letters (A-Z), lowercase letters (a-z), numbers (0-9), underscore (_), dash (-).")]
        [MaxLength(20, ErrorMessage = $"{ValidationFieldPlaceholder} can be maximum 20 characters.")]
        public string CaseId { get; set; }

        [UsaRequired]
        [Display(Name = "Search Reason")]
        public String SearchReason { get; set; }
    }
}
