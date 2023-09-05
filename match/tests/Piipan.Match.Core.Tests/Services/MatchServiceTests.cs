using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchServiceTests
    {
        [Fact]
        public async Task AddRecord_FailsAfterRetryLimit()
        {
            // Arrange
            const int retries = 10;

            var logger = Mock.Of<ILogger<MatchService>>();
            var matchIdService = Mock.Of<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchDao>();
            var record = new MatchDbo();

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchDbo>()))
                .ThrowsAsync(new DuplicateMatchIdException());

            var service = new MatchService(matchRecordDao.Object, matchIdService, logger);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddNewMatchRecord(record));
            matchRecordDao.Verify(m => m.AddRecord(It.IsAny<MatchDbo>()), Times.Exactly(retries));
        }

        [Fact]
        public async Task AddRecord_ThrowsOtherPostgresExceptions()
        {
            var logger = Mock.Of<ILogger<MatchService>>();
            var matchIdService = Mock.Of<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchDao>();
            var exception = new PostgresException("foo", "bar", "baz", PostgresErrorCodes.SyntaxError);
            var record = new MatchDbo();

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchDbo>()))
                .ThrowsAsync(exception);

            var service = new MatchService(matchRecordDao.Object, matchIdService, logger);

            // Act + Assert
            await Assert.ThrowsAsync<PostgresException>(() => service.AddNewMatchRecord(record));
            matchRecordDao.Verify(m => m.AddRecord(It.IsAny<MatchDbo>()), Times.Once);
        }

        [Fact]
        public async Task AddRecord_ReturnsMatchId()
        {
            // Arrange
            var logger = Mock.Of<ILogger<MatchService>>();
            var matchIdService = new Mock<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchDao>();
            var record = new MatchDbo();
            var id = "foo";

            matchIdService
                .Setup(m => m.GenerateId())
                .Returns(id);

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchDbo>()))
                .ReturnsAsync((MatchDbo r) => r.MatchId);

            var service = new MatchService(matchRecordDao.Object, matchIdService.Object, logger);

            // Act
            var result = await service.AddNewMatchRecord(record);

            // Act + Assert
            Assert.True(result == id);
        }
    }
}
