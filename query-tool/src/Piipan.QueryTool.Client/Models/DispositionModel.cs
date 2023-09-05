using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.API.Validation;
using static Piipan.Shared.API.Validation.ValidationConstants;

namespace Piipan.QueryTool.Client.Models
{
    public class DispositionModel
    {
        [Display(Name = "Initial Action Date")]
        [UsaRequiredIf(
            nameof(FinalDisposition), "", $"{ValidationFieldPlaceholder} is required because a Final Disposition has been selected",
            nameof(InitialActionTaken), "", $"{ValidationFieldPlaceholder} is required"
        )]
        [UsaDate]
        public DateTime? InitialActionAt { get; set; }

        private string _initialActionTaken;
        [Display(Name = "Initial Action Taken")]
        [UsaRequiredIf(
            nameof(FinalDisposition), "", $"{ValidationFieldPlaceholder} is required because a Final Disposition has been selected",
            nameof(InitialActionAt), "", $"{ValidationFieldPlaceholder} is required because a date has been selected"
        )]
        public string InitialActionTaken
        {
            get => _initialActionTaken;
            set
            {
                _initialActionTaken = value;
                InitialActionChanged?.Invoke();
            }
        }

        private bool? _invalidMatch = null;
        public bool? InvalidMatch
        {
            get => _invalidMatch;
            set
            {
                _invalidMatch = value;
                InvalidMatchChanged?.Invoke();
            }
        }

        [JsonIgnore]
        public Action InvalidMatchChanged { get; set; }
        [JsonIgnore]
        public Action InitialActionChanged { get; set; }

        [Display(Name = "Invalid Match Reason")]
        [UsaRequiredIf(
            nameof(InvalidMatch), "True", $"{ValidationFieldPlaceholder} is required because Invalid Match has been selected"
        )]
        public string? InvalidMatchReason { get; set; }
        [Display(Name = "Reason for Other")]
        [MaxLength(250, ErrorMessage = $"{ValidationFieldPlaceholder} can be maximum 250 characters.")]
        [UsaRequiredIf(
            nameof(InvalidMatchReason), "Other", $"{ValidationFieldPlaceholder} is required because Other Invalid Match Reason has been selected"
        )]
        public string? OtherReasoningForInvalidMatch { get; set; }

        [Display(Name = "Final Disposition Taken")]
        [UsaRequiredIf(nameof(FinalDispositionDate), "", $"{ValidationFieldPlaceholder} is required because a date has been selected")]
        public string FinalDisposition { get; set; }

        [Display(Name = "Final Disposition Date")]
        [UsaRequiredIf(nameof(FinalDisposition), "", $"{ValidationFieldPlaceholder} is required")]
        // If the Match Date is not set this won't validate. We set it in MatchDetail.razor.
        // So this is client-side validation ONLY. Server-side validation will also occur in
        // AddEventApi's AddEvent method.
        [UsaMinimumDate(nameof(MatchDate), ErrorMessage = "@@@ cannot be before the match date of {0}")]
        [UsaDate]
        public DateTime? FinalDispositionDate { get; set; }

        public bool? VulnerableIndividual { get; set; }
        public string State { get; set; }

        public DateTime? MatchDate { get; set; }

        public DispositionModel() { }
        public DispositionModel(Disposition disposition)
        {
            this.InitialActionAt = disposition.InitialActionAt;
            this.InitialActionTaken = disposition.InitialActionTaken;
            this.FinalDisposition = disposition.FinalDisposition;
            this.FinalDispositionDate = disposition.FinalDispositionDate;
            this.InvalidMatch = disposition.InvalidMatch;
            this.InvalidMatchReason = disposition.InvalidMatchReason;
            this.OtherReasoningForInvalidMatch = disposition.OtherReasoningForInvalidMatch;
            this.VulnerableIndividual = disposition.VulnerableIndividual;
            this.State = disposition.State;
        }

        /// <summary>
        /// Converts this client disposition object back into a Disposition object to be sent to the API
        /// </summary>
        /// <returns></returns>
        public Disposition ToDisposition()
        {
            return new Disposition
            {
                FinalDisposition = this.FinalDisposition,
                FinalDispositionDate = this.FinalDispositionDate,
                InitialActionAt = this.InitialActionAt,
                InitialActionTaken = this.InitialActionTaken,
                InvalidMatch = this.InvalidMatch,
                InvalidMatchReason = this.InvalidMatchReason,
                OtherReasoningForInvalidMatch = this.OtherReasoningForInvalidMatch,
                VulnerableIndividual = this.VulnerableIndividual
            };
        }
    }
}
