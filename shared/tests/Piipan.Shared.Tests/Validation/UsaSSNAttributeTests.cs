using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaSSN attribute
    /// </summary>
    public class UsaSSNAttributeTests
    {
        /// <summary>
        /// Verify SSN has a valid format given multiple inputs
        /// </summary>
        /// <param name="ssn">The value of the associated field</param>
        /// <param name="isValidExpected">Whether or not we expect this field to have an error after the value is entered</param>
        [Theory]
        [InlineData("123-12-1234", true)]
        [InlineData("12-123-1234", false)]
        [InlineData("123121234", false)]
        [InlineData("123-12-123", false)]
        [InlineData("123-12-123A", false)]
        [InlineData("", true)] // empty will work. If it's required, a seperate required attribute should be used in addition to this
        public void IsValidFormat(string ssn, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Act
            var result = attribute.IsValid(ssn);

            // Assert
            Assert.Equal(isValidExpected, result);
            if (!isValidExpected)
            {
                Assert.Equal(ValidationConstants.SSNInvalidFormatMessage, attribute.ErrorMessage);
            }
        }

        /// <summary>
        /// Validate that if the first 3 digits are invalid, we receive an error message
        /// </summary>
        [Theory]
        [InlineData("123-12-1234", true)]
        [InlineData("000-12-1234", false)]
        [InlineData("666-12-1234", false)]
        [InlineData("900-12-1234", false)]
        [InlineData("999-12-1234", false)]
        public void IsValidFirstThreeDigits(string ssn, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Act
            var result = attribute.IsValid(ssn);

            // Assert
            Assert.Equal(isValidExpected, result);
            if (!isValidExpected)
            {
                Assert.Equal(string.Format(ValidationConstants.SSNInvalidFirstThreeDigitsMessage, ssn[0..3]), attribute.ErrorMessage);
            }
        }

        /// <summary>
        /// Validate that if the middle 2 digits are invalid, we receive an error message
        /// </summary>
        [Theory]
        [InlineData("123-01-1234", true)]
        [InlineData("123-00-1234", false)]
        public void IsValidMiddleTwoDigits(string ssn, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Act
            var result = attribute.IsValid(ssn);

            // Assert
            Assert.Equal(isValidExpected, result);
            if (!isValidExpected)
            {
                Assert.Equal(ValidationConstants.SSNInvalidMiddleTwoDigitsMessage, attribute.ErrorMessage);
            }
        }

        /// <summary>
        /// Validate that if the last 4 digits are invalid, we receive an error message
        /// </summary>
        [Theory]
        [InlineData("123-12-0001", true)]
        [InlineData("123-12-0000", false)]
        public void IsValidLastFourDigits(string ssn, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Act
            var result = attribute.IsValid(ssn);

            // Assert
            Assert.Equal(isValidExpected, result);
            if (!isValidExpected)
            {
                Assert.Equal(ValidationConstants.SSNInvalidLastFourDigitsMessage, attribute.ErrorMessage);
            }
        }
    }
}
