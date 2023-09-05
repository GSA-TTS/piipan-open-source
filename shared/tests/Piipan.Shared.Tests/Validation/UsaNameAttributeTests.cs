using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaRequired attribute
    /// </summary>
    public class UsaNameAttributeTests
    {
        /// <summary>
        /// Verify it is either valid or not given multiple inputs
        /// </summary>
        /// <param name="value">The value of the associated field</param>
        /// <param name="isValidExpected">Whether or not we expect this field to have an error after the value is entered</param>
        [Theory]
        [InlineData("Lynn", true)]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("Garcia Sr.", true)]
        [InlineData("-&123", false)]
        [InlineData("García", false)]
        public void IsValid(string value, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaNameAttribute();

            // Act
            var result = attribute.IsValid(value);

            // Assert
            Assert.Equal(isValidExpected, result);
        }

        /// <summary>
        /// Validate that if an error occurs that the correct error message is returned
        /// </summary>
        [Fact]
        public void CorrectErrorMessageForMustStartWithALetter()
        {
            // Arrange
            var attribute = new UsaNameAttribute();

            // Act
            var result = attribute.IsValid("-&123");

            // Assert
            Assert.False(result);
            Assert.Equal(ValidationConstants.MustStartWithALetter, attribute.ErrorMessage);
        }

        /// <summary>
        /// Validate that if an error occurs that the correct error message is returned
        /// </summary>
        [Fact]
        public void CorrectErrorMessageForInvalidCharacter()
        {
            // Arrange
            var attribute = new UsaNameAttribute();

            // Act
            var result = attribute.IsValid("García");

            // Assert
            Assert.False(result);
            Assert.Equal(string.Format(ValidationConstants.InvalidCharacterInNameMessage, "í", "García"), attribute.ErrorMessage);
        }
    }
}
