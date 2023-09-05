using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Shared.Database;
using Piipan.Shared.Tests.Mocks;
using Xunit;

namespace Piipan.Match.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class MatchResEventDaoTests : DbFixture
    {
       
        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        [Fact]
        public async Task AddEvent_FindsCorrectRecord()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchResEvents();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();
                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());
                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                var match = new MatchDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                var record = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                int id = await dao.AddEvent(record);
                record.Id = id;

                // Assert
                Assert.True(HasMatchResEvent(record));
            }
        }

        [Fact]
        public async void GetEvents_FindsAllRecordsInAscOrder()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                // parent match
                var match = new MatchDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                // related match events
                var record1 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                };
                var record2 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"closed\"}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                await dao.AddEvent(record1);
                await dao.AddEvent(record2);

                var result = await dao.GetEventsByMatchId(matchId);
                result = result.ToList();

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Equal(result.ElementAt(0).Delta, record1.Delta);
                Assert.Equal(result.ElementAt(1).Delta, record2.Delta);
                Assert.True(result.ElementAt(0).InsertedAt < result.ElementAt(1).InsertedAt); // asc order
            }
        }

        [Fact]
        public async void GetEvents_FindsAllRecordsInDescOrder()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                // parent match
                var match = new MatchDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                // related match events
                var record1 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                };
                var record2 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"closed\"}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                await dao.AddEvent(record1);
                await dao.AddEvent(record2);

                var result = await dao.GetEventsByMatchId(matchId, false);
                result = result.ToList();

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Equal(result.ElementAt(0).Delta, record2.Delta);
                Assert.Equal(result.ElementAt(1).Delta, record1.Delta);
                Assert.True(result.ElementAt(1).InsertedAt < result.ElementAt(0).InsertedAt); // desc order

                conn.Close();
            }
        }

        [Fact]
        public async void GetEvents_ReturnsEmptyAryWhenNotFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                // Act
                var result = await dao.GetEventsByMatchId("foo", false);
                result = result.ToList();

                // Assert
                Assert.Empty(result);

                conn.Close();
            }
        }

        [Fact]
        public async void GetEvents_ReturnsExpectedEventValues()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchId = idService.GenerateId();

                var match = new MatchDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                // related match events
                var mre = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                await dao.AddEvent(mre);

                // Assert
                var results = await dao.GetEventsByMatchId(matchId, false);
                var result = results.First();
                Assert.Equal(matchId, result.MatchId);
                Assert.Equal("noreply@email.example", result.Actor);
                Assert.Equal("ea", result.ActorState);
                Assert.Equal("{\"status\": \"open\"}", result.Delta);
                Assert.True(new DateTime() < result.InsertedAt); // greater than default datetime (1/1/0001 12:00:00 AM)

                conn.Close();
            }
        }

        [Fact]
        public async void GetEventsByMatchIDs_ReturnsExpectedEventValues()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var matches = matchIds.Select(id => new MatchDbo
                {
                    MatchId = id,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                });
                // related match events
                var mres = matchIds.Select(id => new MatchResEventDbo
                {
                    MatchId = id,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                });

                // Act
                foreach (var match in matches)
                {
                    await matchRecordDao.AddRecord(match);
                }
                foreach (var mre in mres)
                {
                    await dao.AddEvent(mre);
                }

                // Assert
                var results = await dao.GetEventsByMatchIDs(matchIds, false);
                Assert.Equal(3, results.Count());
                foreach (var result in results)
                {
                    Assert.Contains(matchIds, id => result.MatchId == id);
                    Assert.Equal("noreply@email.example", result.Actor);
                    Assert.Equal("ea", result.ActorState);
                    Assert.Equal("{\"status\": \"open\"}", result.Delta);
                    Assert.True(new DateTime() < result.InsertedAt); // greater than default datetime (1/1/0001 12:00:00 AM)
                }

                conn.Close();
            }
        }

        [Fact]
        public async void GetEventsByMatchIDs_ReturnsEmptyListWhenNoRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var matches = matchIds.Select(id => new MatchDbo
                {
                    MatchId = id,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                });

                // Act
                // Add match records, but no events
                foreach (var match in matches)
                {
                    await matchRecordDao.AddRecord(match);
                }

                // Assert
                var results = await dao.GetEventsByMatchIDs(matchIds, false);
                Assert.Empty(results);

                conn.Close();
            }
        }

        [Fact]
        public async void UpdateMatchRecordsNotifiedAt()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var matches = matchIds.Select(id => new MatchDbo
                {
                    MatchId = id,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                });

                // Act
                // Add match records, with one event
                foreach (var match in matches)
                {
                    await matchRecordDao.AddRecord(match);
                    var record = new MatchResEventDbo
                    {
                        MatchId = match.MatchId,
                        Actor = "noreply@email.example",
                        ActorState = "ea",
                        Delta = "{}",
                    };

                    // Act
                    int id = await dao.AddEvent(record);
                }


                // Assert
                var eventsBeforeUpdate = await dao.GetEventsByMatchIDs(matchIds);
                foreach (var eventBeforeUpdate in eventsBeforeUpdate)
                {
                    Assert.Null(eventBeforeUpdate.NotifiedAt);
                }

                // Act - Update match records notified at
                await dao.UpdateMatchRecordsNotifiedAt(eventsBeforeUpdate.Select(n => n.Id).ToArray());

                // Assert events notified at got updated
                var eventsAfterUpdate = await dao.GetEventsByMatchIDs(matchIds);
                foreach (var eventAfterUpdate in eventsAfterUpdate)
                {
                    Assert.NotNull(eventAfterUpdate.NotifiedAt);
                }

                conn.Close();
            }
        }

        [Fact]
        public async void GetEventsNotNotified_NoneExist_UpdatedWithinLast30Minutes()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var match = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                // Add match records, with one event
                await matchRecordDao.AddRecord(match);
                var record = new MatchResEventDbo
                {
                    MatchId = match.MatchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{}",
                };

                int id = await dao.AddEvent(record);

                // Assert - No results because the events were added within the last 30 minutes
                var eventsNotNotified = await dao.GetEventsNotNotified();
                Assert.Empty(eventsNotNotified);

                conn.Close();
            }
        }

        [Fact]
        public async void GetEventsNotNotified_OneExists_UpdatedAnHourAgo()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var match = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                // Add match records, with one event
                await matchRecordDao.AddRecord(match);
                var record = new MatchResEventDbo
                {
                    MatchId = match.MatchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{}",
                };

                // Act
                int id = await dao.AddEvent(record);
                UpdateMatchResEventCreateDateTo1HourAgo(id);


                // Assert - One result because the event was added over 30 minutes ago
                var eventsNotNotified = await dao.GetEventsNotNotified();
                Assert.Single(eventsNotNotified);

                conn.Close();
            }
        }

        [Fact]
        public async void GetEventsNotNotified_MultipleExists_OneUpdatedAnHourAgo()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchDao>>();

                var dao = new MatchResEventDao(logger, DbManager());
                var matchRecordDao = new MatchDao(DbManager());

                var idService = new MatchIdService();
                var matchIds = Enumerable.Range(0, 3).Select(_ => idService.GenerateId()).ToList();

                var match = new MatchDbo
                {
                    MatchId = idService.GenerateId(),
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };

                // Act
                // Add match records, with 2 events
                await matchRecordDao.AddRecord(match);
                var record = new MatchResEventDbo
                {
                    MatchId = match.MatchId,
                    Actor = "noreply@email.example",
                    ActorState = "ea",
                    Delta = "{}",
                };

                // Add two events
                int id1 = await dao.AddEvent(record);
                UpdateMatchResEventCreateDateTo1HourAgo(id1);

                int id2 = await dao.AddEvent(record);

                // Assert - Two results because one was added over 30 minutes ago.
                // The other one was added more recently, but should still get picked up by this function
                var eventsNotNotified = await dao.GetEventsNotNotified();
                Assert.Equal(2, eventsNotNotified.Count());

                conn.Close();
            }
        }
    }
}
