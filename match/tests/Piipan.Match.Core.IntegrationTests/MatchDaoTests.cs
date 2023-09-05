using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Shared.Database;
using Piipan.Shared.Tests.Mocks;
using Xunit;

namespace Piipan.Match.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class MatchDaoTests : DbFixture
    {
        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        [Fact]
        public async Task AddRecord()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var record = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                await dao.AddRecord(record);

                // Assert
                Assert.True(HasRecord(record));
            }
        }

        [Fact]
        public async Task AddRecord_MatchIdCollision()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var record = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                await dao.AddRecord(record);

                // Assert (re-insert same record)
                await Assert.ThrowsAsync<DuplicateMatchIdException>(() => dao.AddRecord(record));
            }
        }

        // AddNewMatchRecord() should let PostgresExceptions bubble up
        // if they are not unique constraint violations
        [Fact]
        public async Task AddRecord_PostgresException()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();

                // Invalid JSON format for Data property
                var record = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{{"
                };

                // Act / Assert
                await Assert.ThrowsAsync<PostgresException>(() => dao.AddRecord(record));
            }
        }

        [Fact]
        public async Task AddRecord_ReturnsMatchId()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchRecords();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var record = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                string result = await dao.AddRecord(record);

                // Assert
                Assert.True(result == record.MatchId);
            }
        }

        [Fact]
        public async Task GetRecords_ReturnsMatchingRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var records = new List<MatchDbo>() {
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    },
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    }
                };

                ClearMatchRecords();
                records.ForEach(r => Insert(r));

                // Act
                var results = (await dao.GetRecordsByHashAndState(records.First())).ToList();

                // Assert
                Assert.True(results.SequenceEqual(records));
            }
        }

        [Fact]
        public async Task GetRecordByMatchId_ReturnsRecordIfFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var id = idService.GenerateId();

                var records = new List<MatchDbo>() {
                    new MatchDbo
                    {
                        MatchId = id,
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    },
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    }
                };

                ClearMatchRecords();
                records.ForEach(r => Insert(r));

                // Act
                var result = await dao.GetRecordByMatchId(id);

                // Assert
                Assert.Equal(result.MatchId, id);
            }
        }

        [Fact]
        public void GetRecordByMatchId_ThrowsExceptionIfNotFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var id = idService.GenerateId();

                var records = new List<MatchDbo>() {
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    },
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    }
                };

                ClearMatchRecords();
                records.ForEach(r => Insert(r));

                // Act
                // Assert
                Assert.ThrowsAsync<System.InvalidOperationException>(() => dao.GetRecordByMatchId(id));
            }
        }

        [Fact]
        public async Task GetMatches_ReturnsRecordsIfFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var records = Enumerable.Range(0, 2).Select(_ =>
                    new MatchDbo
                    {
                        MatchId = idService.GenerateId(),
                        Hash = "foo",
                        HashType = "ldshash",
                        Initiator = "ea",
                        States = new string[] { "ea", "eb" },
                        Data = "{}"
                    }).ToList();


                ClearMatchRecords();
                records.ForEach(r => Insert(r));

                // Act
                var results = await dao.GetMatches();

                // Assert
                Assert.Equal(records, results);
            }
        }

        [Fact]
        public async Task GetMatches_ReturnsNoRecordsIfNotFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchDao(DbManager());

                ClearMatchRecords();

                // Act
                var results = await dao.GetMatches();

                // Assert
                Assert.Empty(results);
            }
        }

        [Fact]
        public async Task GetMatchesById_ReturnsMutlipleRecordsIfFound()
        {
            using var conn = SetupGetMatchesById(out MatchDao dao);

            // Act
            var results = await dao.GetMatchesById(new string[] { "0", "2" });

            // Assert
            Assert.Equal(2, results.Count());
            Assert.NotNull(results.FirstOrDefault(n => n.MatchId == "0"));
            Assert.Null(results.FirstOrDefault(n => n.MatchId == "1"));
            Assert.NotNull(results.FirstOrDefault(n => n.MatchId == "2"));
        }

        [Fact]
        public async Task GetMatchesById_ReturnsOnly1RecordIfFound()
        {
            using var conn = SetupGetMatchesById(out MatchDao dao);

            // Act
            var results = await dao.GetMatchesById(new string[] { "1", "3" });

            // Assert
            Assert.Single(results);
            Assert.Null(results.FirstOrDefault(n => n.MatchId == "0"));
            Assert.NotNull(results.FirstOrDefault(n => n.MatchId == "1"));
            Assert.Null(results.FirstOrDefault(n => n.MatchId == "2"));
            Assert.Null(results.FirstOrDefault(n => n.MatchId == "3"));
        }

        [Fact]
        public async Task GetMatchesById_ReturnsOnlyNoRecordsIfNotFound()
        {
            using var conn = SetupGetMatchesById(out MatchDao dao);

            // Act
            var results = await dao.GetMatchesById(new string[] { "3", "4" });

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMatchesById_ReturnsEmptyWhenNoMatchIDs()
        {
            using var conn = SetupGetMatchesById(out MatchDao dao);

            // Act
            var results = await dao.GetMatchesById(new string[] { });

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMatchesById_ReturnsEmptyWhenNullMatchIDs()
        {
            using var conn = SetupGetMatchesById(out MatchDao dao);

            // Act
            var results = await dao.GetMatchesById(null);

            // Assert
            Assert.Empty(results);
        }

        private DbConnection SetupGetMatchesById(out MatchDao dao)
        {
            var conn = Factory.CreateConnection();
            // Arrange
            conn.ConnectionString = ConnectionString;
            conn.Open();

            var logger = Mock.Of<ILogger<MatchDao>>();
            dao = new MatchDao(DbManager());
            var idService = new MatchIdService();
            var records = Enumerable.Range(0, 3).Select(i =>
                new MatchDbo
                {
                    MatchId = i.ToString(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                }).ToList();


            ClearMatchRecords();
            records.ForEach(r => Insert(r));
            return conn;
        }
    }
}
