using System;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Core.Services;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class ParticipantPublishSearchMetricTests
    {

        [Fact]
        public async void ParticipantPublishUploadMetric_Sucess()
        {
            // Arrange

            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");
            var logger = Mock.Of<ILogger<ParticipantPublishSearchMetric>>();
            var participantPublishSearchdMetric = new ParticipantPublishSearchMetric(logger);
            Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();
            participantPublishSearchdMetric._client = publisherClientMock.Object;
            ParticipantSearchMetrics participantSearchMetrics = new ParticipantSearchMetrics();
            ParticipantSearch metric = new ParticipantSearch()
            {
                State = "ea",
                SearchReason = It.IsAny<string>(),
                SearchFrom = It.IsAny<string>(),
                MatchCreation = It.IsAny<string>(),
                MatchCount = It.IsAny<int>(),
                SearchedAt = DateTime.UtcNow

            };
            participantSearchMetrics.Data.Add(metric);
            // Act
            await participantPublishSearchdMetric.PublishSearchMetrics(participantSearchMetrics);
            publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s => s.EventType == "Upload to the database"), default));
        }

        [Fact]
        public async void ParticipantPublishUploadMetric_FailsDuringInitialization()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EventGridMetricSearchEndPoint", null);
            Environment.SetEnvironmentVariable("EventGridMetricSearchKeyString", null);

            var logger = new Mock<ILogger<ParticipantPublishSearchMetric>>();
            var participantPublishUploadMetric = new ParticipantPublishSearchMetric(logger.Object);

            ParticipantSearchMetrics participantSearchMetrics = new ParticipantSearchMetrics();
            ParticipantSearch metric = new ParticipantSearch()
            {
                State = "ea",
                SearchReason = It.IsAny<string>(),
                SearchFrom = It.IsAny<string>(),
                MatchCreation = It.IsAny<string>(),
                MatchCount = It.IsAny<int>(),
                SearchedAt = DateTime.UtcNow

            };
            participantSearchMetrics.Data.Add(metric);

            // Act
            await Assert.ThrowsAsync<NullReferenceException>(() => participantPublishUploadMetric.PublishSearchMetrics(participantSearchMetrics));

            logger.Verify(x => x.Log(
               It.Is<LogLevel>(l => l == LogLevel.Error),
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to publish Participant Search metrics event to EventGrid.")),
               It.IsAny<Exception>(),
               It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EventGridMetricSearchEndPoint and EventGridMetricSearchKeyString environment variables are not set")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }
    }
}
