using Piipan.Shared.Helpers;
using Xunit;

namespace Piipan.Shared.Tests.Helpers
{
    public class JsonHelperTests
    {
        class DummyClass
        {
            public string Prop1 { get; set; }
        }

        [Fact]
        public void TryParse_ReturnsObject_WhenValidJson()
        {
            // Arrange
            string jsonToVerify = "{ \"Prop1\": \"test\" }";

            // Act
            var dummyValue = JsonHelper.TryParse<DummyClass>(jsonToVerify);

            // Assert
            Assert.Equal("test", dummyValue.Prop1);
        }

        [Fact]
        public void TryParse_ReturnsDefaultObject_WhenInvalidJson()
        {
            // Arrange
            string jsonToVerify = "{ \"InvalidProp\": \"test\" }";

            // Act
            var dummyValue = JsonHelper.TryParse<DummyClass>(jsonToVerify);

            // Assert
            Assert.Null(dummyValue);
        }
    }
}
