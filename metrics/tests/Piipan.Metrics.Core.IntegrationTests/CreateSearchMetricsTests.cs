using Piipan.Metrics.Func.Collect;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;


namespace Piipan.Metrics.Core.IntegrationTests
{
    public class CreateSearchMetricsTests
    {

        private ParticipantSearchMetricsEvent MockEvent(DateTime eventTime, string state)
        {
            ParticipantSearchMetricsEvent gridEvent = new ParticipantSearchMetricsEvent()
            {
                Data = new ParticipantSearch()
                {
                    State = state,
                    MatchCount = 1,
                    SearchFrom = "eb",
                    SearchReason = "other",
                    SearchedAt = eventTime
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

            var searchMetricsApi = new Mock<IParticipantSearchWriterApi>();
            searchMetricsApi
                .Setup(m => m.AddSearchMetrics(
                    It.IsAny<ParticipantSearch>()))
                .ReturnsAsync(1);

            var function = new CreateSearchMetrics(searchMetricsApi.Object);

            // Act
            await function.Run(MockEvent(now, "ea"), logger.Object);

            // Assert
            searchMetricsApi.Verify(m => m.AddSearchMetrics(It.IsAny<ParticipantSearch>()), Times.Once);
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

            var searchMetricsApi = new Mock<IParticipantSearchWriterApi>();
            searchMetricsApi
                .Setup(m => m.AddSearchMetrics(
                    It.IsAny<ParticipantSearch>()))
                .ReturnsAsync(1);

            var function = new CreateSearchMetrics(searchMetricsApi.Object);


            // Act //Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() => function.Run(null, logger.Object));

        }

        [Fact]
        public async Task Run_SearchWriterApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var searchMetricsApi = new Mock<IParticipantSearchWriterApi>();
            searchMetricsApi
                .Setup(m => m.AddSearchMetrics(
                    It.IsAny<ParticipantSearch>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new CreateSearchMetrics(searchMetricsApi.Object);

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
