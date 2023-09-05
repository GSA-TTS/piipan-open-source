using System;
using System.Collections.Generic;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;
using Moq;
using Xunit;
using Piipan.Metrics.Core.Builders;
using System.Threading.Tasks;
using Piipan.Metrics.Core.Models;

namespace Piipan.Metrics.Core.Tests.Services
{
    public class ParticipantSearchServiceTests
    {

        [Fact]
        public async Task AddParticipantSearchRecord()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var searchDao = new Mock<IParticipantSearchDao>();
            searchDao
                .Setup(m => m.AddParticipantSearchRecord(It.IsAny<ParticipantSearchDbo>()))
                .ReturnsAsync(1);
          
            var service = new ParticipantSearchService(searchDao.Object);

            // Act
            var nRows = await service.AddSearchMetrics(new ParticipantSearch());

            // Assert
            Assert.Equal(1, nRows);
            searchDao.Verify(m => m.AddParticipantSearchRecord(It.IsAny<ParticipantSearchDbo>()), Times.Once);
        }

    }
}
