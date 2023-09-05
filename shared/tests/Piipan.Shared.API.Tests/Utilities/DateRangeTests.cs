using System;
using Xunit;

namespace Piipan.Shared.API.Utilities
{
    public class DateRangeTests
    {
        [Fact]
        public void Constructor_SetsValues()
        {
            var start = new DateTime(2021, 1, 1);
            var end = new DateTime(2021, 1, 31);
            // Arrange
            var range = new DateRange(start, end);

            // Act / Assert
            Assert.Equal(start, range.Start);
            Assert.Equal(end, range.End);
        }

        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var range = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };

            // Act / Assert
            Assert.False(range.Equals(null));
        }

        [Fact]
        public void Equals_WrongType()
        {
            // Arrange
            var range = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };
            var notRange = new
            {
                value = 1
            };

            // Act / Assert
            Assert.False(range.Equals(notRange));
        }

        [Fact]
        public void Equals_HashCode_StartMismatch()
        {
            // Arrange
            var range = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };
            var rangeMistmatch = new DateRange
            {
                Start = new DateTime(2021, 1, 2),
                End = new DateTime(2021, 1, 31)
            };

            // Act / Assert
            Assert.False(range.Equals(rangeMistmatch));
            Assert.NotEqual(range.GetHashCode(), rangeMistmatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_EndMismatch()
        {
            // Arrange
            var range = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };
            var rangeMistmatch = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 30)
            };

            // Act / Assert
            Assert.False(range.Equals(rangeMistmatch));
            Assert.NotEqual(range.GetHashCode(), rangeMistmatch.GetHashCode());
        }

        [Fact]
        public void Equals_HashCode_BoundsMatch()
        {
            // Arrange
            var range = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };
            var rangeMistmatch = new DateRange
            {
                Start = new DateTime(2021, 1, 1),
                End = new DateTime(2021, 1, 31)
            };

            // Act / Assert
            Assert.True(range.Equals(rangeMistmatch));
            Assert.Equal(range.GetHashCode(), rangeMistmatch.GetHashCode());
        }
    }
}
