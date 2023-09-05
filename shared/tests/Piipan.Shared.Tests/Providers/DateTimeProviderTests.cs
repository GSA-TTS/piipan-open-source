using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Piipan.Shared.Extensions;
using Xunit;

namespace Piipan.Shared.Tests.Providers
{
    /// <summary>
    /// Set of test cases that fully test the DateTimeProvider class
    /// </summary>
    public class DateTimeProviderTests
    {
        TimeZoneInfo DefaultTestTimeZone = TimeZoneInfo.CreateCustomTimeZone("t1", TimeSpan.FromHours(-5), "Testing Time", "Testing Time");

        /// <summary>
        /// When the time zone is found on the system, verify that the TryGetTimeZoneInfo returns true and outputs the time zone.
        /// </summary>
        [Fact]
        public void DateTimeProvider_TryParseTimeZone_SucceedsWhenValid()
        {
            // Arrange
            var knownId = TimeZoneInfo.Local.Id;
            var provider = new DateTimeProvider();

            // Act
            bool isFound = provider.TryGetTimeZoneInfo(knownId, out var timeZoneInfo);

            // Assert
            Assert.True(isFound);
            Assert.NotNull(timeZoneInfo);
        }

        /// <summary>
        /// When the time zone is NOT found on the system, verify that the TryGetTimeZoneInfo returns false and no TimeZoneInfo output.
        /// </summary>
        [Fact]
        public void DateTimeProvider_TryParseTimeZone_FailsWhenInvalid()
        {
            // Arrange
            var provider = new DateTimeProvider();

            // Act
            bool isFound = provider.TryGetTimeZoneInfo("INVALIDTIMEZONEID", out var timeZoneInfo);

            // Assert
            Assert.False(isFound);
            Assert.Null(timeZoneInfo);
        }

        /// <summary>
        /// Verify the ToEasternTime converts the time when "Eastern Standard Time" exists on the system
        /// </summary>
        [Fact]
        public void DateTimeProvider_ConvertsToEastern_WhenEasterStandardTime_Exists()
        {
            // Arrange
            var myDateUtc = DateTimeOffset.Parse("2022-11-23T00:00:00+00").UtcDateTime;
            var provider = new Mock<DateTimeProvider>();

            TimeZoneInfo expectedTimezone = DefaultTestTimeZone;
            provider.Setup(n => n.TryGetTimeZoneInfo("Eastern Standard Time", out expectedTimezone)).Returns(true);
            provider.Setup(n => n.ToEasternTime(It.IsAny<DateTime?>())).CallBase();

            // Act
            var dateInEasternTime = provider.Object.ToEasternTime(myDateUtc);

            // Assert
            Assert.Equal("2022-11-22 07:00:00 PM", dateInEasternTime.ToString("yyyy-MM-dd hh:mm:ss tt"));
        }

        /// <summary>
        /// Verify the ToEasternTime converts the time when "Eastern Standard Time" does NOT exist on the system but "America/New_York" does.
        /// </summary>
        [Fact]
        public void DateTimeProvider_ConvertsToEaster_WhenAmericaNewYork_Exists()
        {
            // Arrange
            var myDateUtc = DateTimeOffset.Parse("2022-11-23T00:00:00+00").UtcDateTime;
            var provider = new Mock<DateTimeProvider>();

            TimeZoneInfo estTimeZone = null;
            provider.Setup(n => n.TryGetTimeZoneInfo("Eastern Standard Time", out estTimeZone)).Returns(false);

            TimeZoneInfo newYorkTimeZone = DefaultTestTimeZone;
            provider.Setup(n => n.TryGetTimeZoneInfo("America/New_York", out newYorkTimeZone)).Returns(true);

            provider.Setup(n => n.ToEasternTime(It.IsAny<DateTime?>())).CallBase();

            // Act
            var dateInEasternTime = provider.Object.ToEasternTime(myDateUtc);

            // Assert
            Assert.Equal("2022-11-22 07:00:00 PM", dateInEasternTime.ToString("yyyy-MM-dd hh:mm:ss tt"));
        }

        /// <summary>
        /// Verify the ToEasternTime throws a TimeZoneNotFound exception when neither "Eastern Standard Time" not "America/New_York" exist on the system.
        /// </summary>
        [Fact]
        public void DateTimeProvider_ThrowsError_WhenEasternAndNewYork_DontExist()
        {
            // Arrange
            var myDateUtc = DateTimeOffset.Parse("2022-11-23T00:00:00+00").UtcDateTime;
            var provider = new Mock<DateTimeProvider>();

            TimeZoneInfo estTimeZone = null;
            provider.Setup(n => n.TryGetTimeZoneInfo("Eastern Standard Time", out estTimeZone)).Returns(false);

            TimeZoneInfo newYorkTimeZone = null;
            provider.Setup(n => n.TryGetTimeZoneInfo("America/New_York", out newYorkTimeZone)).Returns(false);

            provider.Setup(n => n.ToEasternTime(It.IsAny<DateTime?>())).CallBase();

            // Act/Assert
            Assert.Throws<TimeZoneNotFoundException>(() => provider.Object.ToEasternTime(myDateUtc));
        }
    }
}
