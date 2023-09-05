using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class CreateBulkUploadMetricsTests
    {
        private string MockEvent(string url, DateTime dt)
        {
            var data = new { eTag = "abc", url = url };
            var mockEvent = new { topic = "abc", subject = "xyz", eventType = "BlobCreated", id = "271", data = data, dataVersion = "v1", eventTime = dt };
            return JsonConvert.SerializeObject(mockEvent);
        }

        private string MockBadEventWithMissingSubjectAndEventType(string url, DateTime dt)
        {
            var data = new { eTag = "abc", url = url };
            var mockEvent = new { topic = "abc",data = data, dataVersion = "v1", eventTime = dt };
            return JsonConvert.SerializeObject(mockEvent);
        }

        [Fact]
        public async Task Run_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()))
                .ReturnsAsync(1);

            var function = new CreateBulkUploadMetrics(uploadApi.Object);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            string queueItem = MockEvent("https://abcsteauploaddev.blob.core.azure.com/upload/example.csv", now);
            
            // Act
            await function.Run(queueItem, logger.Object);

            // Assert
            uploadApi.Verify(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Theory]
        [InlineData("badurl", "State not found")] // malformed url, can't parse the state
        [InlineData("https://eupload", "State not found")] // state is only one character
        public async Task Run_BadUrl(string url, string expectedLogMessage)
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()))
                .ReturnsAsync(1);

            var function = new CreateBulkUploadMetrics(uploadApi.Object);

            string queueItem = MockEvent(url, now);

            // Act
            await Assert.ThrowsAsync<FormatException>(() => function.Run(queueItem, logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()), Times.Never);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedLogMessage),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }


        [Fact]
        public async Task Run_UploadApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new CreateBulkUploadMetrics(uploadApi.Object);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string queueItem = MockEvent("https://abcsteauploaddev.blob.core.azure.com/upload/example.csv", now);

            // Act
            await Assert.ThrowsAsync<Exception>(() => function.Run(queueItem, logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Run_BadEventThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();

            var function = new CreateBulkUploadMetrics(uploadApi.Object);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string queueItem = JsonConvert.SerializeObject(MockBadEventWithMissingSubjectAndEventType("https://somethingeaupload", now), serializerSettings);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(() => function.Run(queueItem, logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUploadMetrics(It.IsAny<ParticipantUpload>()), Times.Never);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Argument is not a valid event grid event."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}