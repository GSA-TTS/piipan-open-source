using FluentValidation.TestHelper;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Validators;
using Xunit;


namespace Piipan.Match.Core.Tests.Validators
{
    public class AddEventRequestValidatorTests
    {
        public AddEventRequestValidator Validator()
        {
            return new AddEventRequestValidator();
        }

        [Fact]
        public void ReturnsErrorWhenDataEmpty()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = null
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data);
        }

        #region InitialActionAt

        [Fact]
        public void ReturnsErrorWhen_InitialActionAtIsEmpty_And_InitialActionTakenIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InitialActionTaken = "Notice Sent"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionAt);
        }

        [Fact]
        public void ReturnsErrorWhen_InitialActionAtIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionAt);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchIsEmpty_And_InvlidMatchReasonIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InvalidMatchReason = "test"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InvalidMatchReason);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchReasonOtherIsEmpty_And_InvlidMatchReasonIsOther()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InvalidMatchReason = "Other"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InvalidMatchReason);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchReasonEmpty_And_InvlidMatchIsTrue()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InvalidMatch = true
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InvalidMatchReason);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchReasonEmpty_And_InvlidMatchReasonOtherIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    OtherReasoningForInvalidMatch = "test"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.OtherReasoningForInvalidMatch);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchReasonNotOther_And_InvlidMatchReasonOtherIsNotNull()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InvalidMatchReason = "test",
                    OtherReasoningForInvalidMatch = "test"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InvalidMatchReason);
        }

        [Fact]
        public void ReturnsErrorWhen_InvalidMatchFalse_And_InvlidMatchReasonOtherIsNotNull()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    OtherReasoningForInvalidMatch = "test",
                    InvalidMatch = false
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.OtherReasoningForInvalidMatch);
        }

        #endregion InitialActionAt
        #region InitialActionTaken
        [Fact]
        public void ReturnsErrorWhen_InitialActionTakenIsEmpty_And_InitialActionAtIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    InitialActionAt = System.DateTime.Now
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionTaken);
        }

        [Fact]
        public void ReturnsErrorWhen_InitialActionTakenIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionTaken);
        }
        #endregion InitialActionTaken
        #region FinalDispositionDate
        [Fact]
        public void ReturnsErrorWhen_FinalDispositionDateIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.FinalDispositionDate);
        }
        #endregion FinalDispositionDate
        #region FinalDisposition
        [Fact]
        public void ReturnsErrorWhen_FinalDispositionIsEmpty_And_FinalDispositionDateIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new Disposition
                {
                    FinalDispositionDate = System.DateTime.Now
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.FinalDisposition);
        }
        #endregion FinalDisposition
    }
}
