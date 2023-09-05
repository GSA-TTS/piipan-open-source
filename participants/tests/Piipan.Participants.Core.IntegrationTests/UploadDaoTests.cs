using System;
using Dapper;
using Dapper.NodaTime;
using Moq;
using Npgsql;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Shared.Database;
using Piipan.Shared.Common;
using Piipan.Shared.TestFixtures;
using Piipan.Shared.Tests.Mocks;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class UploadDaoTests : DbFixture
    {
        
        private IDatabaseManager<ParticipantsDbManager> DbManager()
        {
            return new SingleDatabaseManager<ParticipantsDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        [Fact]
        public async void GetLatestUpload()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();

                string uploadId = "test_etag";
                InsertUpload(uploadId, conn);

                var expected = GetLastUploadId(conn);

                var dao = new UploadDao(DbManager());

                // Act
                var result = await dao.GetLatestUpload();

                // Assert
                Assert.Equal(expected, result.Id);

                conn.Close();
            }
        }

        [Fact]
        public async void GetUploadById()
        {
            string uploadId1 = "test_etag";
            string uploadId2 = "test_etag_2";

            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();

                InsertUpload(uploadId1, conn);
                InsertUpload(uploadId2, conn);

                var dao = new UploadDao(DbManager());

                // Act
                var upload1 = await dao.GetUploadById(uploadId1);
                var upload2 = await dao.GetUploadById(uploadId2);

                // Assert
                Assert.NotEqual(upload1, upload2);
                Assert.True(upload1.CreatedAt < upload2.CreatedAt);
                Assert.Equal(uploadId1, upload1.UploadIdentifier);
                Assert.Equal(uploadId2, upload2.UploadIdentifier);

                conn.Close();
            }
        }

        [Fact]
        public async void GetLatestUpload_ThrowsIfNone()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();

                ClearUploads(conn);

                var dao = new UploadDao(DbManager());

                // Act / Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => dao.GetLatestUpload());
                conn.Close();
            }
        }

        [Fact]
        public async void AddUpload()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();

                // Arrange
                var dao = new UploadDao(DbManager());

                // Act
                var result = await dao.AddUpload("test-etag");

                // Assert
                Assert.Equal(GetLastUploadId(conn), result.Id);
                conn.Close();
            }
        }

        [Fact]
        public async void UpdateUpload()
        {
            string uploadId1 = "test_etag";

            using (var conn = TestFixtureDataSourceHelper.CreateConnectionWithNodaTime(ConnectionString))
            {
                // Arrange
                conn.Open();
                var dao = new UploadDao(DbManager());

                InsertUpload(uploadId1, conn);

                var upload = await dao.GetUploadById(uploadId1);
                Assert.Equal(UploadStatuses.COMPLETE.ToString(), upload.Status);

                // Act
                upload.Status = UploadStatuses.FAILED.ToString();
                await dao.UpdateUpload(upload);

                // Assert
                var updatedUpload = await dao.GetUploadById(uploadId1);
                Assert.Equal(UploadStatuses.FAILED.ToString(), upload.Status);
                conn.Close();
            }
        }
    }
}
