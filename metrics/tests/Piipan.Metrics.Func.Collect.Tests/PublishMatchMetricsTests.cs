using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class PublishMatchMetricsTests
    {
        private ParticipantMatchMetricsEvent MockEvent(DateTime eventTime, string State)
        {
            ParticipantMatchMetricsEvent gridEvent = new ParticipantMatchMetricsEvent()
            {
                Data = new ParticipantMatchMetrics()
                {
                    InitState = "ea",
                    MatchId = "foo",
                    MatchingState = "eb",
                    Status = "status"
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

            var matchMetricsApi = new Mock<IParticipantMatchWriterApi>();
            matchMetricsApi
                .Setup(m => m.PublishMatchMetrics(
                    It.IsAny<ParticipantMatchMetrics>()))
                .ReturnsAsync(1);

            var function = new PublishMatchMetrics(matchMetricsApi.Object);

            // Act
            await function.Run(MockEvent(now, "ea"), logger.Object);

            // Assert
            matchMetricsApi.Verify(m => m.PublishMatchMetrics(It.IsAny<ParticipantMatchMetrics>()), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Run_DB_BadParticipantMatchMetrics()
        {
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var matchMetricsApi = new Mock<IParticipantMatchWriterApi>();
            matchMetricsApi
                .Setup(m => m.PublishMatchMetrics(
                    It.IsAny<ParticipantMatchMetrics>()))
                .ReturnsAsync(1);

            var function = new PublishMatchMetrics(matchMetricsApi.Object);


            // Act //Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => function.Run(null, logger.Object));

        }

        [Fact]
        public async Task Run_MatchWriterApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var matchMetricsApi = new Mock<IParticipantMatchWriterApi>();
            matchMetricsApi.Setup(m => m.PublishMatchMetrics(
                    It.IsAny<ParticipantMatchMetrics>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new PublishMatchMetrics(matchMetricsApi.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => function.Run(MockEvent(now, "ea"), logger.Object));

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

    }
}