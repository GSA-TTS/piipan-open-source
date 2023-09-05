using System;
using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    public class UsaMinimumDateAttributeTests
    {
        public class DummyModel
        {
            public DateTime? SourceDate { get; set; }

            [UsaMinimumDate(nameof(SourceDate))]
            public DateTime? TargetDate { get; set; }
        }

        /// <summary>
        /// Validate a UsaMinimumDate attribute attached to a dummy model using various parameters. Must use strings as parameters due to XUnit not allowing
        /// us to put DateTimes into InlineData
        /// </summary>
        [Theory]
        [InlineData(null, null, false)] // No error thrown when both source and target property is null
        [InlineData(null, "07/21/2022", false)] // No error thrown when source property is null
        [InlineData("07/21/2022", null, false)] // No error thrown when target property is null (this attribute does not make this field required!)
        [InlineData("07/21/2022", "07/21/2022", false)] // No error thrown when target property is equal to source property
        [InlineData("07/21/2022", "07/22/2022", false)] // No error thrown when target property is greater than source property
        [InlineData("07/21/2022", "07/20/2022", true)] // Error thrown when target property is smaller than source property
        public void UsaMinimumDate_ValidateTests(string prop1Value, string prop2Value, bool hasError)
        {
            // Arrange
            var attribute = new UsaMinimumDateAttribute(nameof(DummyModel.SourceDate)) { ErrorMessage = "Date is not greater than {0}" };

            var dummyModel = new DummyModel()
            {
                SourceDate = prop1Value == null ? null : DateTime.Parse(prop1Value),
                TargetDate = prop2Value == null ? null : DateTime.Parse(prop2Value)
            };

            // Act
            var validationResult = attribute.GetValidationResult(dummyModel.TargetDate, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(hasError, !string.IsNullOrEmpty(validationResult?.ErrorMessage));
            if (hasError)
            {
                Assert.Equal($"Date is not greater than 07/21/2022", validationResult.ErrorMessage);
            }
        }
    }
}
