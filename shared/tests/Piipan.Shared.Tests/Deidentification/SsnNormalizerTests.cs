using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Piipan.Shared.Deidentification.Tests
{
    public class SsnNormalizerTests
    {
        private SsnNormalizer _ssnNormalizer;

        public SsnNormalizerTests()
        {
            _ssnNormalizer = new SsnNormalizer();
        }

        // correct format
        [Theory]
        [InlineData("25-46-5417")] // missing Area digit
        [InlineData("425-6-5417")] // missing Group digit
        [InlineData("425-46-541")] // missing Serial digit
        [InlineData("425 46-5417")] // missing hyphen
        public void Run_ThrowsOnIncorrectFormat(string ssn)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            Assert.Contains("social security number must have a 3-digit area number, a 2-digit group number, and a 4-digit serial number, in this order, all separated by a hyphen", exception.Message.ToLower());
        }

        // Area numbers 000, 666, and 900-999 are invalid
        [Theory]
        [InlineData("000-11-1111")] // area number 000
        [InlineData("666-11-1111")] // area number 666
        [InlineData("901-11-1111")] // area number in 900-999 range
        [InlineData("900-11-1111")] // area number in 900-999 range
        [InlineData("999-11-1111")] // area number in 900-999 range
        public void Run_ThrowsOnInvalidAreaNumbers(string ssn)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run(ssn));
            Assert.Contains("the first three numbers of ssn cannot be 000, 666, or between 900-999", exception.Message.ToLower());
        }

        // Group number 00 is invalid
        [Fact]
        public void Run_ThrowsOnInvalidGroupNumber()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run("111-00-1111"));
            Assert.Contains("the middle two numbers of ssn cannot be 00", exception.Message.ToLower());
        }

        // Serial number 0000 is invalid
        [Fact]
        public void Run_ThrowsOnInvalidSerialNumber()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _ssnNormalizer.Run("111-11-0000"));
            Assert.Contains("the last four numbers of ssn cannot be 0000", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("425-46-5417")]
        [InlineData("423-32-7469")]
        [InlineData("561-40-4846")]
        public void Run_AcceptsValidSSNs(string source)
        {
            string result = _ssnNormalizer.Run(source);
            Assert.Equal(source, result);
        }
    }
}
