using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Models;
using Piipan.Metrics.Core.Services;
using Xunit;

namespace Piipan.Metrics.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    {
        [Fact]
        public async Task GetUploads()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ReturnsAsync(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });
            var metaBuilder = new Mock<IMetaBuilder>();
            metaBuilder.Setup(m => m.SetFilter(It.IsAny<ParticipantUploadRequestFilter>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetTotal(It.IsAny<long>())).Returns(metaBuilder.Object);

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder.Object);

            // Act
            var response = await service.GetUploads(
                new ParticipantUploadRequestFilter { State = "somestate" });

            // Assert
            Assert.Single(response.Data);
            Assert.Single(response.Data, (u) => u.State == "somestate");
            Assert.Single(response.Data, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public async Task GetLatestUploadsByState()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestSuccessfulUploadsByState())
                .ReturnsAsync(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var response = await service.GetLatestUploadsByState();

            // Assert
            Assert.Single(response.Data);
            Assert.Single(response.Data, (u) => u.State == "somestate");
            Assert.Single(response.Data, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public async Task AddUpload()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload(It.IsAny<ParticipantUploadDbo>()))
                .ReturnsAsync(1);
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var nRows = await service.AddUploadMetrics(new ParticipantUpload());

            // Assert
            Assert.Equal(1, nRows);
            uploadDao.Verify(m => m.AddUpload(It.IsAny<ParticipantUploadDbo>()), Times.Once);
        }

        [Fact]
        public async Task GetUploadStatistics()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ReturnsAsync(new ParticipantUploadStatistics
                {
                    TotalFailure = 1,
                    TotalComplete = 2
                });
            var metaBuilder = new Mock<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder.Object);

            // Act
            var response = await service.GetUploadStatistics(
                new ParticipantUploadStatisticsRequest { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.Date, HoursOffset = -5 });

            // Assert
            Assert.Equal(1, response.TotalFailure);
            Assert.Equal(2, response.TotalComplete);
        }
    }
}