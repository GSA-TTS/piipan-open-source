using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NodaTime;
using NpgsqlTypes;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Participants.Core.Tests
{
    public class DateRangeListHandlerTests
    {
        [Fact]
        public void ParseThrowsForBadInput()
        {
            // Arrange 
            var handler = new DateRangeListHandler();

            // Act / Assert
            Assert.Throws<InvalidCastException>(() => handler.Parse("not a List<DateRange> "));
        }

        [Fact]
        public void Parse_NpgsqlRangeOfDateTime_ReturnsList()
        {
            // Arrange
            var handler = new DateRangeListHandler();

            // Act
            var response = handler.Parse(new NpgsqlRange<DateTime>[] { new NpgsqlRange<DateTime>(DateTime.Now, DateTime.UtcNow) });

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Single(response);
        }

        [Fact]
        public void Parse_NpgsqlRangeOfDateTime_HandlesExclusiveBounds()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var value = new NpgsqlRange<DateTime>[] {
                new NpgsqlRange<DateTime>(
                    new DateTime(2022,1,1), false,
                    new DateTime(2022,1,31), false
                )
            };

            // Act
            var response = handler.Parse(value);
            var responseAsList = response.ToList();

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Single(response);
            Assert.Equal(new DateTime(2022, 1, 2), responseAsList.First().Start);
            Assert.Equal(new DateTime(2022, 1, 30), responseAsList.First().End);
        }

        [Fact]
        public void Parse_NpgsqlRangeOfDateTime_HandlesInclusiveBounds()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var value = new NpgsqlRange<DateTime>[] {
                new NpgsqlRange<DateTime>(
                    new DateTime(2022,1,1), true,
                    new DateTime(2022,1,31), true
                )
            };

            // Act
            var response = handler.Parse(value);
            var responseAsList = response.ToList();

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Single(response);
            Assert.Equal(new DateTime(2022, 1, 1), responseAsList.First().Start);
            Assert.Equal(new DateTime(2022, 1, 31), responseAsList.First().End);
        }

        [Fact]
        public void ParseReturnsEmptyListForDBNull()
        {
            // Arrange
            var handler = new DateRangeListHandler();

            // Act
            var response = handler.Parse(DBNull.Value);

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Empty(response);
        }

        [Fact]
        public void ParseReturnsEmptyList()
        {
            // Arrange
            var handler = new DateRangeListHandler();

            // Act
            var response = handler.Parse(new NpgsqlRange<DateTime>[] { });

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Empty(response);
        }

        [Fact]
        public void Parse_DateIntervals_ReturnsList()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            DateInterval[] intervals = new DateInterval[1];
            var value = new DateInterval(LocalDate.FromDateTime(DateTime.Now), LocalDate.FromDateTime(DateTime.UtcNow));
            intervals[0] = value;

            // Act
            var response = handler.Parse(intervals);

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Single(response);
        }

        [Fact]
        public void Parse_DateIntervals_HandlesInclusiveBounds()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            DateInterval[] intervals = new DateInterval[1];
            var value = new DateInterval(LocalDate.FromDateTime(new DateTime(2022, 1, 1)), LocalDate.FromDateTime(new DateTime(2022, 1, 31)));
            intervals[0] = value;

            // Act
            var response = handler.Parse(intervals);
            var responseAsList = response.ToList();

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Single(response);
            Assert.Equal(new DateTime(2022, 1, 1), responseAsList.First().Start);
            Assert.Equal(new DateTime(2022, 1, 31), responseAsList.First().End);
        }

        [Fact]
        public void Parse_DbNull_ReturnsEmptyList()
        {
            // Arrange
            var handler = new DateRangeListHandler();

            // Act
            var response = handler.Parse(new NpgsqlRange<DateTime>[] { });

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Empty(response);
        }

        [Fact]
        public void SetValueSetsParameterValue()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var parameter = new Mock<IDbDataParameter>();

            // Act
            handler.SetValue(parameter.Object, new List<DateRange> { new DateRange(
                DateTime.Now,
                DateTime.Now) });
            // Assert
            parameter.VerifySet(m => m.Value = It.IsAny<string>(), Times.Once);
        }

        [Fact]
        public void SetValueReturnsStringUsingInclusiveBounds()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var parameter = new Mock<IDbDataParameter>();
            var range = new List<DateRange> {
                new DateRange(new DateTime(2022,1,1), new DateTime(2022,1,2))
            };
            var expected = "{\"[2022-01-01,2022-01-02]\"}";

            // Act
            handler.SetValue(parameter.Object, range);

            // Assert
            parameter.VerifySet(p => p.Value = expected, Times.Once);
        }

        [Fact]
        public void SetValueWithMultipleRangesReturnsStringUsingInclusiveBounds()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var parameter = new Mock<IDbDataParameter>();
            var range = new List<DateRange> {
                new DateRange(new DateTime(2022,1,1), new DateTime(2022,1,2)),
                new DateRange(new DateTime(2022,2,1), new DateTime(2022,2,2))
            };
            var expected = "{\"[2022-01-01,2022-01-02]\",\"[2022-02-01,2022-02-02]\"}";

            // Act
            handler.SetValue(parameter.Object, range);

            // Assert
            parameter.VerifySet(p => p.Value = expected, Times.Once);
        }

        [Fact]
        public void SetValueReturnsEmptyArray()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var parameter = new Mock<IDbDataParameter>();
            var range = new List<DateRange> { };
            var expected = "{}";

            // Act
            handler.SetValue(parameter.Object, range);

            // Assert
            parameter.VerifySet(p => p.Value = expected, Times.Once);
        }
    }
}
