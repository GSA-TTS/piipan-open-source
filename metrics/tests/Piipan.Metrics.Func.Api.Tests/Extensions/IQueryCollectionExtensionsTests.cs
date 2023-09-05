using Piipan.Metrics.Func.Api.Extensions;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Piipan.Metrics.Func.Api.Tests.Extensions
{
    public class IQueryCollectionExtensionsTests
    {
        [Fact]
        public void ParseString_Present()
        {
            // Arrange
            var collection = new Mock<IQueryCollection>();

            collection
                .Setup(m => m[It.IsAny<string>()])
                .Returns("value");

            // Act
            var result = collection.Object.ParseString("key");

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void ParseString_NotPresent()
        {
            // Arrange
            var collection = new Mock<IQueryCollection>();

            collection
                .Setup(m => m[It.IsAny<string>()])
                .Returns(StringValues.Empty);

            // Act
            var result = collection.Object.ParseString("key");

            // Assert
            Assert.Equal(StringValues.Empty, result);
        }

        [Fact]
        public void ParseInt_Present()
        {
            // Arrange
            var collection = new Mock<IQueryCollection>();

            collection
                .Setup(m => m[It.IsAny<string>()])
                .Returns("52");

            // Act
            var result = collection.Object.ParseInt("key");

            // Assert
            Assert.Equal(52, result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 10)]
        public void ParseInt_NotPresent(int defaultValue, int expectedResult)
        {
            // Arrange
            var collection = new Mock<IQueryCollection>();

            collection
                .Setup(m => m[It.IsAny<string>()])
                .Returns(StringValues.Empty);

            // Act
            var result = collection.Object.ParseInt("key", defaultValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 10)]
        public void ParseInt_NonNumeric(int defaultValue, int expectedResult)
        {
            // Arrange
            var collection = new Mock<IQueryCollection>();

            collection
                .Setup(m => m[It.IsAny<string>()])
                .Returns("notanumber");

            // Act
            var result = collection.Object.ParseInt("key", defaultValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}