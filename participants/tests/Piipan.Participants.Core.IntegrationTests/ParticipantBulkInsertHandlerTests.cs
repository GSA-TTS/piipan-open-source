using System;
using System.Linq;
using Dapper;
using Dapper.NodaTime;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Shared.TestFixtures;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantBulkInsertHAndlerTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void LoadParticipants(int nParticipants)
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var handler = new ParticipantBulkInsertHandler(logger);
                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId(conn)).ToList();

                // Act
                await handler.LoadParticipants(participants, conn, "participants");

                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p, conn));
                });
            }
        }
    }
}
