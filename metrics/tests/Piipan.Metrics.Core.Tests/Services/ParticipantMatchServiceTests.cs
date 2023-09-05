using System;
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
    public class ParticipantMatchServiceTests
    {

        [Fact]
        public async Task AddParticipantMatchMetricsRecord()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var matchhDao = new Mock<IParticipantMatchDao>();
            matchhDao
                .Setup(m => m.AddParticipantMatchRecord(It.IsAny<ParticipantMatchDbo>()))
                .ReturnsAsync(1);

            var service = new ParticipantMatchService(matchhDao.Object);

            // Act
            var nRows = await service.AddMatchMetrics(new ParticipantMatchMetrics());

            // Assert
            Assert.Equal(1, nRows);
            matchhDao.Verify(m => m.AddParticipantMatchRecord(It.IsAny<ParticipantMatchDbo>()), Times.Once);
        }
        [Fact]
        public async Task UpdateParticipantMatchMetricsRecord()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var matchhDao = new Mock<IParticipantMatchDao>();
            matchhDao
                .Setup(m => m.UpdateParticipantMatchRecord(It.IsAny<ParticipantMatchDbo>()))
                .ReturnsAsync(1);

            var service = new ParticipantMatchService(matchhDao.Object);

            // Act
            var nRows = await service.UpdateMatchMetrics(new ParticipantMatchMetrics());

            // Assert
            Assert.Equal(1, nRows);
            matchhDao.Verify(m => m.UpdateParticipantMatchRecord(It.IsAny<ParticipantMatchDbo>()), Times.Once);
        }

    }
}
