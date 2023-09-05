using System;
using Piipan.Shared.API.Validation;
using Xunit;

namespace Piipan.Shared.Tests.Validation
{
    public class UsaDateAttributeTests
    {
        [Theory]
        [InlineData("01/01/2000", null, 1999, 12, 31, false, "must be after 01-01-2000")]
        [InlineData("01/01/2000", null, 2000, 1, 1, true, "")]
        [InlineData(null, "12/31/2050", 1700, 1, 1, true, "")] // when min value is null, don't validate min value
        [InlineData("01/01/1900", null, 3000, 1, 1, true, "")] // when max value is null, don't validate max value
        [InlineData(null, "12/31/1999", 1999, 12, 31, true, "")]
        [InlineData(null, "12/31/1999", 2000, 1, 1, false, "must be before 12-31-1999")]
        [InlineData("01/01/1900", "Today", 2000, 1, 1, true, "")]
        [InlineData("01/01/1900", "Today", 3000, 1, 1, false, "must be between 01-01-1900 and today's date")]
        [InlineData("Today", "12/31/2050", 2050, 1, 1, true, "")]
        [InlineData("Today", "12/31/2050", 2022, 1, 1, false, "must be between today's date and 12-31-2050")]
        [InlineData("01/01/2000", "12/31/2050", 2051, 1, 1, false, "must be between 01-01-2000 and 12-31-2050")]
        [InlineData("01/01/1900", "12/31/2050", 2051, 1, 1, false, "must have a year between 1900 and 2050")]
        public void IsValid(string min, string max, int y, int m, int d, bool isValidExpected, string errorMessage)
        {
            // Arrange
            var attribute = new UsaDateAttribute(min, max);

            // Act
            var result = attribute.IsValid(new DateTime(y, m, d));

            // Assert
            Assert.Equal(isValidExpected, result);
            if (!isValidExpected)
            {
                Assert.EndsWith(errorMessage, attribute.ErrorMessage);
            }
        }
    }
}