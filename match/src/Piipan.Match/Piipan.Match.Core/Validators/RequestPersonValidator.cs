using FluentValidation;
using Piipan.Match.Api.Models;
using Piipan.Shared.API.Enums;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class RequestPersonValidator : AbstractValidator<RequestPerson>
    {
        public RequestPersonValidator()
        {
            const string HashRegex = "^[a-z0-9]{128}$";
            const string AlphanumericRegex = "^[A-Za-z0-9-_]+$";
            const string AlphanumericRegexWithEmpty = "^[A-Za-z0-9-_]*$";

            RuleFor(q => q.LdsHash).Matches(HashRegex).NotNull();
            RuleFor(q => q.ParticipantId).NotNull();
            RuleFor(q => q.ParticipantId).Matches(AlphanumericRegex);
            RuleFor(q => q.ParticipantId).MaximumLength(20).WithName("Participant Id");
            RuleFor(x => x.SearchReason).IsEnumName(typeof(ValidSearchReasons)).NotNull();
            RuleFor(q => q.CaseId).Matches(AlphanumericRegexWithEmpty);
            RuleFor(q => q.CaseId).MaximumLength(20).WithName("Case Id");
        }
    }
}