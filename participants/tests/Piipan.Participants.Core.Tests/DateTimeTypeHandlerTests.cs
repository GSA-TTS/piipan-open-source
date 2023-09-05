using Moq;
using NodaTime;
using Piipan.Shared.Database;
using System;
using System.Data;
using Xunit;

namespace Piipan.Participants.Core.Tests
{
    public class DateTimeTypeHandlerTests
    {
        [Fact]
        public void ParseThrowsForBadInput()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            // Act / Assert
            Assert.Throws<DataException>(() => handler.Parse(5));
        }

        [Fact]
        public void ParseThrowsForInvalidFormattedDateString()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            // Act / Assert
            Assert.Throws<FormatException>(() => handler.Parse($"Invalid input"));
        }

        [Fact]
        public void ParseValidDateTimeString()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            DateTime currentTime = DateTime.UtcNow;
            var dateTimeString = currentTime.ToString();

            var expected = DateTime.Parse(((string)dateTimeString));

            var result = handler.Parse(dateTimeString);

            // Act / Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ParseDateTime()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            DateTime currentTime = DateTime.UtcNow;

            var result = handler.Parse(currentTime);

            // Act / Assert
            Assert.Equal(currentTime, result);
        }

        [Fact]
        public void ParseInstant()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            DateTime currentTime = DateTime.UtcNow;
            Instant instant = Instant.FromDateTimeUtc(currentTime);
            
            var result = handler.Parse(instant);

            // Act / Assert
            Assert.Equal(currentTime, result);
        }

        [Fact]
        public void ParseLocalDate()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            var currentTime = DateTime.UtcNow;
            var instant = LocalDate.FromDateTime(currentTime);
            var expectedValue = instant.ToDateTimeUnspecified();
            var result = handler.Parse(instant);

            // Act / Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void SetValue()
        {
            // Arrange 
            var handler = new DateTimeTypeHandler();

            DateTime currentTime = DateTime.Now;
            var parameter = new Mock<IDbDataParameter>();

            var localDate = LocalDateTime.FromDateTime(currentTime);

            // Act
            handler.SetValue(parameter.Object, currentTime);
            // Assert
            parameter.VerifySet(m => m.Value = localDate, Times.Once);
        }
    }
}
