using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Services;
using Piipan.Shared.Cryptography;
using Piipan.Shared.TestFixtures;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantServiceTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public ParticipantServiceTests()
        {
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        [Theory]
        [InlineData(2)]
        public async Task AddUploadAndParticipants(int nParticipants)
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);

                var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

                var participantsDatabaseManager = helper.DbManager(ConnectionString);
                var uploadDao = new UploadDao(participantsDatabaseManager);
                var participantUploadService = new ParticipantUploadService(uploadDao, participantPublishUploadMetric.Object);
                var participantDao = new ParticipantDao(bulkInserter, logger, participantsDatabaseManager);

                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

                ParticipantService service = new ParticipantService(participantDao, participantUploadService, null, serviceLogger, cryptographyClient);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId(conn));

                var upload = await uploadDao.AddUpload("test-etag");

                // Act
                await service.AddParticipants(participants, upload, "ea", null);

                long lastUploadId = GetLastUploadId(conn);

                // updatiing lds_hash with encryption

                participants.ToList().ForEach(p =>
                {
                    p.LdsHash = cryptographyClient.EncryptToBase64String(p.LdsHash);
                    p.CaseId = cryptographyClient.EncryptToBase64String(p.CaseId);
                    p.ParticipantId = cryptographyClient.EncryptToBase64String(p.ParticipantId);
                    p.UploadId = p.UploadId;
                });

                // Assert
                participants.ToList().ForEach(p =>
                {
                    p.UploadId = lastUploadId;
                    var exists = HasParticipant(p, conn);
                    Assert.True(exists);
                });

                conn.Close();
            }
        }


        [Theory]
        [InlineData(5)]
        public async Task AfterException_AddParticipantsRollsTranactionBack(int nParticipants)
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                // Arrange
                conn.Open();
                ClearParticipants(conn);

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);

                var participantsDatabaseManager = helper.DbManager(ConnectionString);
                var participantDao = new ParticipantDao(bulkInserter, logger, participantsDatabaseManager);
                var uploadDao = new UploadDao(participantsDatabaseManager);

                var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

                var participantUploadService = new ParticipantUploadService(uploadDao, participantPublishUploadMetric.Object);

                ParticipantService service = new ParticipantService(participantDao, participantUploadService, null, serviceLogger, cryptographyClient);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId(conn));
                participants.Last().LdsHash = null; //Cause the db commit to fail due to a null hash value

                long lastUploadId = GetLastUploadId(conn);

                var upload = await uploadDao.AddUpload("test-etag");

                try
                {
                    // Act
                    await service.AddParticipants(participants, upload, "ea", (ex) => { return ex.Message; });
                    throw new Exception("Test should have failed because of participant with null ldsHash value");
                }
                catch (Exception)
                {
                    long expectedNewUploadId = ++lastUploadId;
                    long actualLastUploadId = GetLastUploadIdWithStatus("COMPLETE", conn);
                    Assert.NotEqual(expectedNewUploadId, actualLastUploadId);


                    // Assert
                    participants.ToList().ForEach(p =>
                    {
                        p.UploadId = lastUploadId;
                        var exists = HasParticipant(p, conn);
                        Assert.False(exists);
                    });

                    long lastFailedUploadId = GetLastUploadIdWithStatus("FAILED", conn);

                    Assert.Equal(expectedNewUploadId, lastFailedUploadId);
                }
                conn.Close();
            }
        }
    }
}
