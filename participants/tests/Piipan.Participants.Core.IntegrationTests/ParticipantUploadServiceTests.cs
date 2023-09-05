using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    /// <summary>
    /// 
    /// </summary>
    [Collection("Core.IntegrationTests")]
    public class ParticipantUploadServiceTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();

        [Fact]
        public async void CreateValidationError_ShouldUpdateUploadWithError()
        {
            var logger = Mock.Of<ILogger<ParticipantDao>>();
            var state = "ea";
            var validationErrorMessage = "ErrorTest";
            var uploadIdentifier = "test-etag";

            var uploadDao = new UploadDao(helper.DbManager(ConnectionString));

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
            participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                    It.IsAny<ParticipantUpload>()))
                    .Returns(Task.CompletedTask);

            var participantUploadService = new ParticipantUploadService(uploadDao, participantPublishUploadMetric.Object);

            var uploadBeforeUpdate = await participantUploadService.GetUploadById(uploadIdentifier);

            // Act
            await participantUploadService.UpdateUpload(uploadBeforeUpdate, state, validationErrorMessage);

            // Assert
            var upload = await uploadDao.GetUploadById(uploadIdentifier);

            Assert.True(upload?.UploadIdentifier == uploadIdentifier);
            Assert.True(upload?.ErrorMessage == validationErrorMessage);
            Assert.True(upload?.Status == UploadStatuses.FAILED.ToString());

            participantPublishUploadMetric.Verify(c => c.PublishUploadMetric(It.IsAny<ParticipantUpload>()), Times.Once);
        }

        [Fact]
        public async void UploadWithNonExistentId_StillPublishesFailureMetric()
        {
            var logger = Mock.Of<ILogger<ParticipantDao>>();
            var state = "ea";
            var validationErrorMessage = "ErrorTest";
            var uploadIdentifier = "invalid_id";

            var uploadDao = new UploadDao(helper.DbManager(ConnectionString));

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
            participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                    It.IsAny<ParticipantUpload>()))
                    .Returns(Task.CompletedTask);

            var participantUploadService = new ParticipantUploadService(uploadDao, participantPublishUploadMetric.Object);

            var uploadBeforeUpdate = new UploadDto() { UploadIdentifier = uploadIdentifier };

            // Act
            await participantUploadService.UpdateUpload(uploadBeforeUpdate, state, validationErrorMessage);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await uploadDao.GetUploadById(uploadIdentifier));

            participantPublishUploadMetric.Verify(c => c.PublishUploadMetric(It.IsAny<ParticipantUpload>()), Times.Once);
        }
    }
}
