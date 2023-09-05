using System;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantPublishUploadMetricTests
    {

        [Fact]
        public async void ParticipantPublishUploadMetric_Sucess()
        {
            // Arrange

            Environment.SetEnvironmentVariable("EventGridEndPoint","http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString","example");
            var logger = Mock.Of<ILogger<ParticipantPublishUploadMetric>>();
            var participantPublishUploadMetric = new ParticipantPublishUploadMetric(logger);
            Mock<EventGridPublisherClient> publisherClientMock = new Mock<EventGridPublisherClient>();
            participantPublishUploadMetric._client = publisherClientMock.Object;

            ParticipantUpload metric = new ParticipantUpload()
            {
                State = "ea",
                Status = UploadStatuses.COMPLETE.ToString(),
                UploadIdentifier = "UploadIdentifier",
                UploadedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                ParticipantsUploaded = 50

            };

            // Act
            await participantPublishUploadMetric.PublishUploadMetric(metric);
            publisherClientMock.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(s=>s.EventType == "Upload to the database"), default));
        }

        [Fact]
        public async void ParticipantPublishUploadMetric_FailsDuringInitialization()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EventGridEndPoint", null);
            Environment.SetEnvironmentVariable("EventGridKeyString", null);

            var logger = new Mock<ILogger<ParticipantPublishUploadMetric>>();
            var participantPublishUploadMetric = new ParticipantPublishUploadMetric(logger.Object);

            ParticipantUpload metric = new ParticipantUpload()
            {
                State = "ea",
                Status = UploadStatuses.COMPLETE.ToString(),
                UploadIdentifier = "UploadIdentifier",
                UploadedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                ParticipantsUploaded = 50

            };

            // Act
            await Assert.ThrowsAsync<NullReferenceException>(() => participantPublishUploadMetric.PublishUploadMetric(metric));
            
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to publish ParticipantUpload metrics event to EventGrid.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }
    }
}
