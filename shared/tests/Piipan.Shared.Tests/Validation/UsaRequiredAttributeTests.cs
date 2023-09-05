using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaRequired attribute
    /// </summary>
    public class UsaRequiredAttributeTests
    {
        /// <summary>
        /// Verify it is either valid or not given multiple inputs
        /// </summary>
        /// <param name="value">The value of the associated field</param>
        /// <param name="isValidExpected">Whether or not we expect this field to have an error after the value is entered</param>
        [Theory]
        [InlineData("value", true)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void IsValid(string value, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaRequiredAttribute();

            // Act
            var result = attribute.IsValid(value);

            // Assert
            Assert.Equal(isValidExpected, result);
        }

        /// <summary>
        /// Validate that if an error occurs that the correct error message is returned
        /// </summary>
        [Fact]
        public void CorrectErrorMessageIsUsed()
        {
            // Arrange
            var attribute = new UsaRequiredAttribute();

            // Assert
            Assert.Equal(ValidationConstants.RequiredMessage, attribute.ErrorMessage);
        }
    }
}
