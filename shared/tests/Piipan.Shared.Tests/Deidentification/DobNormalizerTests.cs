using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Piipan.Shared.Deidentification.Tests
{
    public class DobNormalizerTests
    {
        private DobNormalizer _dobNormalizer;

        public DobNormalizerTests()
        {
            _dobNormalizer = new DobNormalizer();
        }

        [Theory]
        [InlineData("98-08-14")] // year is not fully specified
        [InlineData("5/15/2002")] // wrong value order, wrong separator character, value is not zero-padded
        [InlineData("2000-11-2")] // day is not zero-padded
        public void Run_ThrowsOnNonISO8601Dates(string date)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            Assert.Contains("dates must be in iso 8601 format using a 4-digit year, a zero-padded month, and zero-padded day", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("2001-02-29")] // invalid date, 2001 is not a leap year
        [InlineData("1991-09-31")] // sept has 30 days
        public void Run_ThrowsOnNonGregorianDates(string date)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            Assert.Contains("date must exist on gregorian calendar", exception.Message.ToLower());
        }

        [Fact]
        public void Run_ThrowsOnDatesTooOld()
        {
            string date = DateTime.Now.AddYears(-131).ToString("yyyy-MM-dd");
            ArgumentException exception = Assert.Throws<ArgumentException>(() => _dobNormalizer.Run(date));
            Assert.Contains($"date should be later than {DobNormalizer.MaxYearsAgo} years ago", exception.Message.ToLower());
        }

        [Theory]
        [InlineData("2000-11-02")]
        public void Run_AllowsValidDates(string date)
        {
            var result = _dobNormalizer.Run(date);
            Assert.Equal(date, result);
        }
    }
}
