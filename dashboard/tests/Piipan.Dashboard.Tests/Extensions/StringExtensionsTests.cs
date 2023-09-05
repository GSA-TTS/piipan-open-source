using Piipan.Dashboard.Client.Helpers;
using Xunit;

namespace Piipan.Dashboard.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("test", "Test")]
        [InlineData("TEST", "Test")]
        [InlineData("Test", "Test")]
        [InlineData("Some String Has Spaces", "Some string has spaces")]
        [InlineData("", "")]
        public void TestToLowerExceptFirstLetter(string inputString, string expectedOutputString)
        {
            Assert.Equal(expectedOutputString, inputString.ToLowerExceptFirstLetter());
        }
    }
}
