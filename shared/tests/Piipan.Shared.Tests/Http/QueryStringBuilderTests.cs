using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Shared.Tests.Http
{
    public class QueryStringBuilderTests
    {
        public class FakeRequestType
        {
            public string RequestMessage { get; set; } = "default";
            public int RequestInt { get; set; } = 2;
        }

        [Fact]
        public void QueryStringBuilder_CreatesQueryString_WhenAllPropertiesFilledIn()
        {
            // Arrange
            var request = new FakeRequestType
            {
                RequestInt = 1,
                RequestMessage = "Test"
            };
            string queryString = QueryStringBuilder.ToQueryString(request);

            Assert.Equal("?RequestMessage=Test&RequestInt=1", queryString);
        }

        [Fact]
        public void QueryStringBuilder_CreatesEmptyQueryString_WhenNoPropertiesFilledIn()
        {
            // Arrange
            var request = new FakeRequestType();
            string queryString = QueryStringBuilder.ToQueryString(request);

            Assert.Equal("", queryString);
        }

        [Fact]
        public void QueryStringBuilder_CreatesQueryString_WhenTypeDefaultsUsed()
        {
            // Arrange
            var request = new FakeRequestType()
            {
                RequestMessage = "",
                RequestInt = 0
            };
            string queryString = QueryStringBuilder.ToQueryString(request);

            Assert.Equal("?RequestMessage=&RequestInt=0", queryString);
        }
    }
}
