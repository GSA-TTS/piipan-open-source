using FluentValidation;
using Piipan.Etl.Func.BulkUpload.Models;

namespace Piipan.Etl.Func.BulkUpload.Validators
{
    /// <summary>
    /// Sets validation rules in accordance with
    /// <c>/etl/docs/csv/import-schema.json</c>.
    /// </summary>
    public class ParticipantValidator : AbstractValidator<ParticipantCsv>
    {
        const string HashRegex = "^[a-z0-9]{128}$";
        const string AlphanumericRegex = "^[A-Za-z0-9-_]+$";
        const string AlphanumericRegexWithEmpty = "^[A-Za-z0-9-_]*$";
        private DateValidateHelper helper => new DateValidateHelper();

        public ParticipantValidator()
        {
            RuleFor(q => q.LdsHash).Matches(HashRegex).NotNull();
            RuleFor(q => q.ParticipantId).NotNull();
            RuleFor(q => q.ParticipantId).Matches(AlphanumericRegex);
            RuleFor(q => q.ParticipantId).MaximumLength(20).WithName("Participant Id");
            RuleFor(q => q.CaseId).Matches(AlphanumericRegexWithEmpty);
            RuleFor(q => q.CaseId).MaximumLength(20).WithName("Case Id");
            RuleFor(q => q.ParticipantClosingDate).Must(field =>
            {
                return helper.IsValidParticipantClosingDate(field);
            });
            RuleFor(q => q.RecentBenefitIssuanceDates).Must(field =>
            {
                return helper.IsValidRecentBenefitIssuanceDates(field);
            });
        }
    }
}