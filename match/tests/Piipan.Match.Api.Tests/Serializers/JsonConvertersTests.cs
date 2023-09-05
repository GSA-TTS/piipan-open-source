using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using Piipan.Match.Api.Serializers;
using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Match.Api.Tests.Serializers
{
    public class JsonConvertersTests
    {
        [Fact]
        public void DateTimeConverter_SetsFormat()
        {
            // Arrange
            var converter = new JsonConverters.DateTimeConverter();

            // Assert
            Assert.Equal("yyyy-MM-dd", converter.DateTimeFormat);
        }

        [Fact]
        public void NullConverter_CanReadIsTrue()
        {
            // Arrange
            var converter = new JsonConverters.NullConverter();

            // Assert
            Assert.True(converter.CanRead);
        }

        [Fact]
        public void NullConverter_CanWriteIsTrue()
        {
            // Arrange
            var converter = new JsonConverters.NullConverter();

            // Assert
            Assert.False(converter.CanWrite);
        }

        [Fact]
        public void NullConverter_CanConvertChecksType()
        {
            // Arrange
            var converter = new JsonConverters.NullConverter();

            // Assert
            Assert.True(converter.CanConvert(typeof(string)));
            Assert.False(converter.CanConvert(typeof(int)));
            Assert.False(converter.CanConvert(typeof(DateTime)));
            Assert.False(converter.CanConvert(typeof(object)));
            Assert.False(converter.CanConvert(typeof(bool)));
        }

        [Fact]
        public void NullConverter_ReadJsonReturnsNullForEmptyInput()
        {
            // Arrange
            var converter = new JsonConverters.NullConverter();
            var reader = new Mock<JsonReader>();
            reader
                .Setup(m => m.Value)
                .Returns((object)null);

            // Act
            var result = converter.ReadJson(reader.Object,
                default(Type),
                default(object),
                default(JsonSerializer));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void NullConverter_ReadJsonReturnsInput()
        {
            // Arrange
            var input = Guid.NewGuid().ToString();
            var converter = new JsonConverters.NullConverter();
            var reader = new Mock<JsonReader>();
            reader
                .Setup(m => m.Value)
                .Returns((object)input);

            // Act
            var result = converter.ReadJson(reader.Object,
                default(Type),
                default(object),
                default(JsonSerializer));

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void NullConverter_WriteJsonThrowsNotImplemented()
        {
            // Arrange
            var converter = new JsonConverters.NullConverter();
            var writer = Mock.Of<JsonWriter>();

            // Assert / Act
            Assert.Throws<NotImplementedException>(() =>
            {
                converter.WriteJson(writer,
                    default(object),
                    default(JsonSerializer));
            });
        }
        
        [Fact]
        public void MonthEndArrayConverter_CanReadIsFalse()
        {
            // Arrange
            var converter = new JsonConverters.MonthEndArrayConverter();

            // Assert
            Assert.False(converter.CanRead);
        }

        [Fact]
        public void MonthEndArrayConverter_CanWriteIsTrue()
        {
            // Arrange
            var converter = new JsonConverters.MonthEndArrayConverter();

            // Assert
            Assert.True(converter.CanWrite);
        }

        [Fact]
        public void MonthEndArrayConverter_CanConvertChecksType()
        {
            // Arrange
            var converter = new JsonConverters.MonthEndArrayConverter();

            // Assert
            Assert.True(converter.CanConvert(typeof(IEnumerable<DateTime>)));
            Assert.False(converter.CanConvert(typeof(DateTime)));
            Assert.False(converter.CanConvert(typeof(int)));
            Assert.False(converter.CanConvert(typeof(string)));
            Assert.False(converter.CanConvert(typeof(object)));
            Assert.False(converter.CanConvert(typeof(bool)));
        }

        [Fact]
        public void MonthEndArrayConverter_ReadJsonThrowsNotImplemented()
        {
            // Arrange
            var converter = new JsonConverters.MonthEndArrayConverter();
            var reader = Mock.Of<JsonReader>();

            // Act / Assert
            Assert.Throws<NotImplementedException>(() =>
            {
                converter.ReadJson(reader,
                    default(Type),
                    default(object),
                    default(JsonSerializer));
            });
        }

        [Fact]
        public void MonthEndArrayConverter_WriteJson()
        {
            // Arrange
            var converter = new JsonConverters.MonthEndArrayConverter();
            var writer = new Mock<JsonWriter>();
            var input = new List<DateTime>
            {
                new DateTime(2020, 1, 1),
                new DateTime(2020, 2, 1)
            };

            // Act
            converter.WriteJson(writer.Object,
                (object)input,
                default(JsonSerializer));

            // Assert
            writer.Verify(m => m.WriteStartArray(), Times.Once);
            writer.Verify(m => m.WriteValue("2020-01"), Times.Once);
            writer.Verify(m => m.WriteValue("2020-02"), Times.Once);
            writer.Verify(m => m.WriteEndArray(), Times.Once);
        }
        [Fact]
        public void MDateRangeConverter_CanReadIsFalse()
        {
            // Arrange
            var converter = new JsonConverters.DateRangeConverter();

            // Assert
            Assert.False(converter.CanRead);
        }

        [Fact]
        public void DateRangeConverter_CanWriteIsTrue()
        {
            // Arrange
            var converter = new JsonConverters.DateRangeConverter();

            // Assert
            Assert.True(converter.CanWrite);
        }

        [Fact]
        public void DateRangeConverter_CanConvertChecksType()
        {
            // Arrange
            var converter = new JsonConverters.DateRangeConverter();

            // Assert
            Assert.True(converter.CanConvert(typeof(IEnumerable<DateRange>)));
            Assert.False(converter.CanConvert(typeof(DateRange)));
            Assert.False(converter.CanConvert(typeof(int)));
            Assert.False(converter.CanConvert(typeof(string)));
            Assert.False(converter.CanConvert(typeof(object)));
            Assert.False(converter.CanConvert(typeof(bool)));
        }

        [Fact]
        public void DateRangeConverter_ReadJsonThrowsNotImplemented()
        {
            // Arrange
            var converter = new JsonConverters.DateRangeConverter();
            var reader = Mock.Of<JsonReader>();

            // Act / Assert
            Assert.Throws<NotImplementedException>(() =>
            {
                converter.ReadJson(reader,
                    default(Type),
                    default(object),
                    default(JsonSerializer));
            });
        }

        [Fact]
        public void DateRangeConverter_WriteJson()
        {
            // Arrange
            var converter = new JsonConverters.DateRangeConverter();
            var writer = new Mock<JsonWriter>();
            var input = new List<DateRange>
            {
                new DateRange( new DateTime(2020, 1, 1), new DateTime(2020, 1, 31)),
                 new DateRange( new DateTime(2020, 2, 1), new DateTime(2020, 2, 28))
            };

            // Act
            converter.WriteJson(writer.Object,
                (object)input,
                new JsonSerializer());

            // Assert
            writer.Verify(m => m.WriteStartArray(), Times.Once);
            writer.Verify(m => m.WriteValue("2020-01-01"), Times.Once);
            writer.Verify(m => m.WriteValue("2020-02-01"), Times.Once);
            writer.Verify(m => m.WriteEndArray(), Times.Once);
        }
    }
}
