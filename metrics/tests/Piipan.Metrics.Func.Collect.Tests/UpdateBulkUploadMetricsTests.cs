using Piipan.Metrics.Func.Collect;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class UpdateBulkUploadMetricsTests
    {

        private ParticipantUploadMetricsEvent MockEvent(DateTime eventTime, string State)
        {
            ParticipantUploadMetricsEvent gridEvent = new ParticipantUploadMetricsEvent()
            {
                Data = new ParticipantUpload()
                {
                    State = "ea",
                    Status = "status",
                    UploadIdentifier = "UploadIdentifier",
                    UploadedAt = eventTime,
                    CompletedAt = eventTime,
                }
            };

            return gridEvent;

        }

        [Fact]
        public async Task Run_DB_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.UpdateUploadMetrics(
                    It.IsAny<ParticipantUpload>()))
                .ReturnsAsync(1);

            var function = new UpdateBulkUploadMetrics(uploadApi.Object);

            // Act
            await function.Run(MockEvent(now, "ea"), logger.Object);

            // Assert
            uploadApi.Verify(m => m.UpdateUploadMetrics(It.IsAny<ParticipantUpload>()), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }



        [Fact]
        public async Task Run_DB_BadParticipantUpload()
        {
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUploadMetrics(
                    It.IsAny<ParticipantUpload>()))
                .ReturnsAsync(1);

            var function = new UpdateBulkUploadMetrics(uploadApi.Object);


            // Act //Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => function.Run(null, logger.Object));

        }

        [Fact]
        public async Task Run_UploadApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi.Setup(m => m.UpdateUploadMetrics(
                    It.IsAny<ParticipantUpload>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new UpdateBulkUploadMetrics(uploadApi.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => function.Run(MockEvent(now, "ea"), logger.Object));

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed updating Bulk Upload status & metrics."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

    }
}