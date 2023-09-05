using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Shared.Api.Tests.Serializers
{
    public class JsonConvertersSharedTests
    {
        [Fact]
        public void DateTimeConverter_SetsFormat()
        {
            // Arrange
            var converter = new JsonConvertersShared.DateTimeConverter();

            // Assert
            Assert.Equal("yyyy-MM-dd", converter.DateTimeFormat);
        }

    }
}
