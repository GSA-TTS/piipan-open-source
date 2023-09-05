using FluentValidation.TestHelper;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Validators;
using System.Linq;
using Xunit;

namespace Piipan.Match.Core.Tests.Validators
{
    public class RequestPersonValidatorTests
    {
        public RequestPersonValidator Validator()
        {
            return new RequestPersonValidator();
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("a3c", true)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcde", true)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eea61389-_&^%$#@!().,:;][{}|", true)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcders", true)]
        [InlineData("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec", false)]
        public void IsValidLdsHash(string value, bool hasValidationErrorForLdsHash)
        {
            // Arrange
            RequestPerson model = new() { LdsHash = value };

            // Act
            var result = Validator().TestValidate(model);

            // Assert
            Assert.Equal(result.Errors.Any(c =>
                        c.PropertyName == nameof(model.LdsHash)),
                                hasValidationErrorForLdsHash);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("foo", true)]
        [InlineData("application", false)]
        [InlineData("new_household_member", false)]
        [InlineData("recertification", false)]
        [InlineData("other", false)]
        public void IsValidSearchReason(string value, bool hasValidationErrorForSearchReason)
        {
            // Arrange
            RequestPerson model = new() { SearchReason = value };

            // Act
            var result = Validator().TestValidate(model);

            // Assert
            Assert.Equal(result.Errors.Any(c =>
                        c.PropertyName == nameof(model.SearchReason)),
                                hasValidationErrorForSearchReason);
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("#", true)]
        [InlineData("$", true)]
        [InlineData("%", true)]
        [InlineData("^", true)]
        [InlineData("&", true)]
        [InlineData("*", true)]
        [InlineData("[", true)]
        [InlineData("{", true)]
        [InlineData("/", true)]
        [InlineData("(", true)]
        [InlineData("!", true)]
        [InlineData(",", true)]
        [InlineData(".", true)]
        [InlineData(";", true)]
        [InlineData(" ", true)]
        [InlineData("\n:", true)]
        [InlineData(null, true)]
        [InlineData("AZaz09-_", false)]
        [InlineData("123456789-123456789-123456789", true)]
        public void IsValidParticipantId(string value, bool hasValidationErrorForParticipantId)
        {
            // Arrange
            RequestPerson model = new() { ParticipantId = value };

            // Act
            var result = Validator().TestValidate(model);

            // Assert
            Assert.Equal(result.Errors.Any(c =>
                        c.PropertyName == nameof(model.ParticipantId)),
                                hasValidationErrorForParticipantId);
        }

        [Theory]
        [InlineData("#", true)]
        [InlineData("$", true)]
        [InlineData("%", true)]
        [InlineData("^", true)]
        [InlineData("&", true)]
        [InlineData("*", true)]
        [InlineData("[", true)]
        [InlineData("{", true)]
        [InlineData("/", true)]
        [InlineData("(", true)]
        [InlineData("!", true)]
        [InlineData(",", true)]
        [InlineData(".", true)]
        [InlineData(";", true)]
        [InlineData(" ", true)]
        [InlineData("\n:", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("AZaz09-_", false)]
        [InlineData("123456789-123456789-123456789", true)]
        public void IsValidCaseId(string value, bool hasValidationErrorForCaseId)
        {
            // Arrange
            RequestPerson model = new() { CaseId = value };

            // Act
            var result = Validator().TestValidate(model);

            // Assert
            Assert.Equal(result.Errors.Any(c =>
                        c.PropertyName == nameof(model.CaseId)),
                                hasValidationErrorForCaseId);
        }
    }
}