using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.TestFixtures;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantDaoTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";

        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();
            services.RegisterKeyVaultClientServices();
            return services.BuildServiceProvider();
        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants(int nParticipants)
        {
            var services = BuildServices();
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);

                var dao = new ParticipantDao(bulkInserter, logger, helper.DbManager(ConnectionString));
                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId(conn));

                // Act
                await dao.AddParticipants(participants);

                // updatiing lds_hash with encryption
                participants = participants.Select(p => new ParticipantDbo(p)
                {
                    UploadId = p.UploadId
                });

                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p, conn));
                });
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(20)]                                              
        public async void GetParticipants(int nMatches)
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var randoms = helper.RandomParticipants(nMatches, GetLastUploadId(conn));
                var participants = randoms.ToList().Select(p =>
                {
                    return new ParticipantDbo
                    {
                        // make the hashes and upload id match for all of them
                        LdsHash = randoms.First().LdsHash,
                        State = randoms.First().State,
                        CaseId = p.CaseId,
                        ParticipantId = p.ParticipantId,
                        ParticipantClosingDate = p.ParticipantClosingDate,
                        RecentBenefitIssuanceDates = p.RecentBenefitIssuanceDates,
                        VulnerableIndividual = p.VulnerableIndividual,
                        UploadId = randoms.First().UploadId
                    };
                });

                participants.ToList().ForEach(p => Insert(p, conn));

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(bulkInserter, logger, helper.DbManager(ConnectionString));

                // Act
                var matches = await dao.GetParticipants("ea", randoms.First().LdsHash, randoms.First().UploadId);

                // Assert
                Assert.True(participants.OrderBy(p => p.CaseId).SequenceEqual(matches.OrderBy(p => p.CaseId)));
            }
        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void DeleteOldParticipants(int nParticipants)
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(bulkInserter, logger, helper.DbManager(ConnectionString));
                InsertUpload("test_etag", conn);
                var participants = helper.RandomParticipants(nParticipants, GetLastUploadIdWithStatus("COMPLETE", conn));

                // Act
                await dao.AddParticipants(participants);

                // Insert New Participants
                InsertUpload("test_etag", conn);
                var participantsNew = helper.RandomParticipants(nParticipants, GetLastUploadIdWithStatus("COMPLETE", conn));

                await dao.AddParticipants(participantsNew);

                // updatiing lds_hash with encryption
                participants = participants.Select(p => new ParticipantDbo(p)
                {
                    UploadId = p.UploadId

                });
                // updatiing lds_hash with encryption
                participantsNew = participantsNew.Select(p => new ParticipantDbo(p)
                {
                    UploadId = p.UploadId

                });

                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p, conn));
                });
                participantsNew.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p, conn));
                });
                // Now Delete Old Participants.

                await dao.DeleteOldParticipantsExcept(string.Empty, GetLastUploadIdWithStatus("COMPLETE", conn));
                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.False(HasParticipant(p, conn));
                });
                participantsNew.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p, conn));
                });

                conn.Close();
            }
        }

       [Fact]
        public async void DeleteOldParticipantLogEntry()
        {


            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = new Mock<ILogger<ParticipantDao>>();   
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(bulkInserter, logger.Object, helper.DbManager(ConnectionString));
                InsertUpload("test_etag", conn);
                var participants = helper.RandomParticipants(2, GetLastUploadIdWithStatus("COMPLETE", conn));

                // Act
                await dao.AddParticipants(participants);

                // Insert New Participants
                InsertUpload("test_etag", conn);
                var participantsNew = helper.RandomParticipants(2, GetLastUploadIdWithStatus("COMPLETE", conn));

                await dao.AddParticipants(participantsNew);
               
                // Now Delete Old Participants.

                await dao.DeleteOldParticipantsExcept(string.Empty, GetLastUploadIdWithStatus("COMPLETE", conn));
                // Assert

                // Assert
                logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Outdated participant cleanup; Cleanup Time")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
            }

        }
    }
}
