using System;
using System.Threading.Tasks;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    { 
        public ParticipantUploadServiceTests()
        {
        }

        [Fact]
        public async Task GetLatestUploadById()
        {
            // Arrange
            const string uploadId = "upload1";
            var createdAt = DateTime.Now;
            var uploadDao = new Mock<IUploadDao>();

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                UploadIdentifier = uploadId
            };

            uploadDao.Setup(m => m.GetUploadById(It.IsAny<string>())).ReturnsAsync(upload);

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantUploadService(uploadDao.Object, participantPublishUploadMetric.Object);

            // Act
            var result = await service.GetUploadById(uploadId);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
        }

        [Fact]
        public async Task GetLatestUpload()
        {
            // Arrange
            const string state = "EA";
            var createdAt = DateTime.Now;
            var uploadDao = new Mock<IUploadDao>();
            
            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
            };

            uploadDao.Setup(m => m.GetLatestUpload(It.IsAny<string>())).ReturnsAsync(upload);

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantUploadService(uploadDao.Object, participantPublishUploadMetric.Object);

            // Act
            var result = await service.GetLatestUpload(state);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
        }

        [Fact]
        public async Task AddUpload()
        {
            // Arrange
            var createdAt = DateTime.Now;
            const string uploadIdentifier = "upload_1";

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                UploadIdentifier = uploadIdentifier
            };

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload(It.IsAny<string>()))
                .ReturnsAsync(upload);

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantUploadService(uploadDao.Object, participantPublishUploadMetric.Object);

            // Act
            
            var result = await service.AddUpload(uploadIdentifier, "ea");

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
            uploadDao.Verify(m => m.AddUpload(It.Is<string>(x=>x==uploadIdentifier)), Times.Once);

            //Assert metric does NOT get published. The new upload metric should be triggered by a BlobCreated event in Azure Storage
            participantPublishUploadMetric.Verify(m => m.PublishUploadMetric(It.IsAny<ParticipantUpload>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUpload()
        {
            // Arrange
            var createdAt = DateTime.Now.AddMinutes(-1);
            const string uploadIdentifier = "upload_1";
            var numParticipants = 10;

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                ParticipantsUploaded = numParticipants,
                UploadIdentifier = uploadIdentifier,
                Status = UploadStatuses.COMPLETE.ToString()
            };

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.UpdateUpload(It.IsAny<IUpload>()))
                .ReturnsAsync(1);

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantUploadService(uploadDao.Object, participantPublishUploadMetric.Object);

            // Act
            var result = await service.UpdateUpload(upload, "ea");

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(1, result);
            uploadDao.Verify(m => m.UpdateUpload(It.Is<IUpload>(x => x.UploadIdentifier == uploadIdentifier)), Times.Once);
            participantPublishUploadMetric.Verify(m => m.PublishUploadMetric(It.Is<ParticipantUpload>(x=>
                x.UploadIdentifier == uploadIdentifier && 
                x.Status == UploadStatuses.COMPLETE.ToString() &&
                x.ParticipantsUploaded == numParticipants &&
                x.UploadedAt == createdAt &&
                x.CompletedAt > createdAt &&
                x.State == "ea")));
        }

        [Fact]
        public async Task UpdateUploadRecordWithFailureMessage()
        {
            // Arrange
            var createdAt = DateTime.Now;
            const string uploadIdentifier = "upload_1";
            var numParticipants = 10;

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                ParticipantsUploaded = numParticipants,
                UploadIdentifier = uploadIdentifier,
                Status = UploadStatuses.COMPLETE.ToString()
            };

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.UpdateUpload(It.IsAny<IUpload>()))
                .ReturnsAsync(1);

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantUploadService(uploadDao.Object, participantPublishUploadMetric.Object);

            // Act
            await service.UpdateUpload(upload, "ea", "The Upload Failed");

            var uploadDto = new UploadDto(upload);

            // Assert
            uploadDao.Verify(m => m.UpdateUpload(It.Is<IUpload>(x => x.UploadIdentifier == uploadIdentifier)), Times.Once);
            participantPublishUploadMetric.Verify(m => m.PublishUploadMetric(It.Is<ParticipantUpload>(x =>
                x.UploadIdentifier == uploadIdentifier &&
                x.Status == UploadStatuses.FAILED.ToString() &&
                x.ParticipantsUploaded == null &&
                x.UploadedAt == createdAt &&
                x.CompletedAt > createdAt &&
                x.State == "ea")));
        }
    }
}
