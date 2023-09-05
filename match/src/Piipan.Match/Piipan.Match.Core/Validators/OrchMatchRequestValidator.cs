using Piipan.Match.Api.Models;
using FluentValidation;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates the whole API match request from a client
    /// </summary>
    public class OrchMatchRequestValidator : AbstractValidator<OrchMatchRequest>
    {
        private readonly int MaxPersonsInRequest = 50;
        public OrchMatchRequestValidator()
        {
            RuleFor(r => r.Data)
                .NotNull()
                .NotEmpty()
                .Must(data => data.Count <= MaxPersonsInRequest)
                .WithMessage($"Data count cannot exceed {MaxPersonsInRequest}");
        }
    }
}
