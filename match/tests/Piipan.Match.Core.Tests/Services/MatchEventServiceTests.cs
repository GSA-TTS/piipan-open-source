using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Metrics.Api;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Enums;
using Piipan.States.Core.Service;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchEventServiceTests
    {
        private const string QueryToolUrl = "https://tts.test";
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private string[] enabledStateList = { "ea", "eb" };
        private string[] enabledStateShortList = { "eb" };

        public MatchEventServiceTests()
        {
            Environment.SetEnvironmentVariable("QueryToolUrl", QueryToolUrl);
        }

        private Mock<IActiveMatchBuilder> BuilderMock(MatchDbo record)
        {
            var recordBuilder = new Mock<IActiveMatchBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(record);

            return recordBuilder;
        }

        private NotificationRecord NotificationRecord = new NotificationRecord()
        {
            MatchEmailDetails = new MatchEmailModel()
            {
                MatchId = "foo",
                InitState = "ea",
                MatchingState = "eb",
                MatchingUrl = It.IsAny<string>(),
            },
            InitiatingStateEmailRecipientsModel = new EmailToModel()
            {
                EmailTo = "Ea@Piipan.gov"
            },
            MatchingStateEmailRecipientsModel = new EmailToModel()
            {
                EmailTo = "Eb@Piipan.gov"
            }
        };

        private Mock<IMatchRecordApi> ApiMock(string matchId = "foo")
        {
            var api = new Mock<IMatchRecordApi>();
            api.Setup(r => r.AddNewMatchRecord(It.IsAny<IMatchDbo>()))
                .ReturnsAsync(matchId);

            return api;
        }

        private Mock<IMatchResEventDao> MatchResEventDaoMock(
            IEnumerable<IMatchResEvent> events
        )
        {
            var mock = new Mock<IMatchResEventDao>();
            mock.Setup(r => r.GetEventsByMatchId(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            return mock;
        }

        private Mock<IParticipantPublishSearchMetric> ParticipantPublishSearchMetricMock()
        {
            var mock = new Mock<IParticipantPublishSearchMetric>();
            mock.Setup(m => m.PublishSearchMetrics(It.IsAny<ParticipantSearchMetrics>()))
                  .Returns(Task.CompletedTask);

            return mock;
        }

        private Mock<IParticipantPublishMatchMetric> ParticipantPublishMatchMetricMock()
        {
            var mock = new Mock<IParticipantPublishMatchMetric>();
            mock.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                  .Returns(Task.CompletedTask);

            return mock;
        }

        private Mock<INotificationService> NotificationServiceMock()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchEmailDetails = new MatchEmailModel()
                {
                    MatchId = It.IsAny<string>(),
                    InitState = It.IsAny<string>(),
                    MatchingState = It.IsAny<string>(),
                    MatchingUrl = It.IsAny<string>(),
                },
                MatchingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = It.IsAny<string>()
                },
                InitiatingStateEmailRecipientsModel = new EmailToModel()
                {
                    EmailTo = It.IsAny<string>()
                }
            };
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.PublishNotificationOnMatchCreation(
                notificationRecord)).Returns(Task.FromResult(true));
            return mock;
        }

        private Mock<IStateInfoService> StateInfoDaoMock()
        {
            var stateInfoDao = new Mock<IStateInfoService>();
            stateInfoDao
                .Setup(r => r.GetDecryptedStates())
                    .ReturnsAsync(new List<StateInfoDbo>()
                    {
                    new StateInfoDbo() { Id = "1", State = "Echo Alpha", StateAbbreviation = "ea" , Email = "Ea@Piipan.gov" , EmailCc = "Ea-cc@Piipan.gov"},
                    new StateInfoDbo() { Id = "2", State = "Echo Bravo", StateAbbreviation = "eb" , Email = "Eb@Piipan.gov" , EmailCc = "Eb-cc@Piipan.gov"},
                    new StateInfoDbo() { Id = "3", State = "Echo Charlie", StateAbbreviation = "ec" , Email = "Ec@Piipan.gov" , EmailCc = "Ec-cc@Piipan.gov"},
                    });
            return stateInfoDao;
        }

        private Mock<IMatchDetailsAggregator> MatchResAggregatorMock(
                    MatchDetailsDto result
                )
        {
            var mock = new Mock<IMatchDetailsAggregator>();
            mock.Setup(r => r.BuildAggregateMatchDetails(
                It.IsAny<IMatchDbo>(),
                It.IsAny<IEnumerable<IMatchResEvent>>()
            ))
            .Returns(result);

            return mock;
        }

        [Fact]
        public async void Resolve_AddsSingleRecord()
        {
            // Arrange
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = "Api",
                SearchReason = null,
                MatchCreation = "New Created Match",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);

            // Assert
            recordApi.Verify(r => r.AddNewMatchRecord(
                It.Is<IMatchDbo>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Once);

            publishSearchMetrics.Verify(r => r.PublishSearchMetrics(
              It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                   r.Data.First().MatchCreation == search.MatchCreation &&
                                                   r.Data.First().SearchReason == search.SearchReason &&
                                                   r.Data.First().SearchFrom == search.SearchFrom)),
                                                   Times.Once);

            publishMatchMetrics.Verify(r => r.PublishMatchMetric(
             It.Is<ParticipantMatchMetrics>(r => r.MatchId == "foo" &&
                                                  r.InitState == record.Initiator
                                                 )),
                                                  Times.Once);
            // Need to be called only when creating new Match Record
            notificationService.Verify(r => r.PublishNotificationOnMatchCreation(It.Is<NotificationRecord>(p => p.MatchEmailDetails.InitState == "Echo Alpha" && p.InitiatingStateEmailRecipientsModel.EmailTo == "Ea@Piipan.gov" && p.MatchingStateEmailRecipientsModel.EmailTo == "Eb@Piipan.gov")), Times.Once);
        }

        [Fact]
        public async void Resolve_AddsManyRecords()
        {
            // Arrange
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() {
                    new ParticipantMatch { LdsHash = "foo", State= "eb" },
                    new ParticipantMatch { LdsHash = "foo", State = "ec" }
                }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());
            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);

            // Assert
            recordApi.Verify(r => r.AddNewMatchRecord(
                It.Is<IMatchDbo>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Exactly(result.Matches.Count()));
        }

        [Fact]
        public async void Resolve_InsertsMatchId()
        {
            // Arrange
            var mockMatchId = "BDC2345";
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(mockMatchId);

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{mockMatchId}", firstMatch.MatchUrl);
            Assert.Equal(mockMatchId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }

        [Fact]
        public async void Resolve_InsertsMostRecentMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var closedMatchId = "CDB5432";
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchDbo>()))
                .ReturnsAsync(new List<MatchDbo> {
                    new MatchDbo {
                        MatchId = openMatchId,
                        CreatedAt = new DateTime(2020,01,02)
                    },
                    new MatchDbo {
                        MatchId = closedMatchId,
                        CreatedAt = new DateTime(2020,01,01)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());
            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{openMatchId}", firstMatch.MatchUrl);
            Assert.Equal(openMatchId, firstMatch.MatchId);
        }

        [Fact]
        public async void Resolve_InsertsOpenMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchDbo>()))
                .ReturnsAsync(new List<MatchDbo> {
                    new MatchDbo {
                        MatchId = openMatchId
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{openMatchId}", firstMatch.MatchUrl);
            Assert.Equal(openMatchId, firstMatch.MatchId);
        }

        [Fact]
        public async void Resolve_InsertsNewMatchIdIfMostRecentRecordIsClosed()
        {
            // Arrange
            var newId = "newId";
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = "Api",
                SearchReason = null,
                MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.NEWMATCH),
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(newId);
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchDbo>()))
                .ReturnsAsync(new List<MatchDbo> {
                    new MatchDbo {
                        MatchId = "closedId",
                        CreatedAt = new DateTime(2020,01,02)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto()
            {
                Status = MatchRecordStatus.Closed
            });

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{newId}", firstMatch.MatchUrl);
            Assert.Equal(newId, firstMatch.MatchId);

            publishSearchMetrics.Verify(r => r.PublishSearchMetrics(
             It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                  r.Data.First().MatchCreation == search.MatchCreation &&
                                                  r.Data.First().SearchReason == search.SearchReason &&
                                                  r.Data.First().SearchFrom == search.SearchFrom)),
             Times.Once);

            notificationService.Verify(r => r.PublishNotificationOnMatchCreation(It.Is<NotificationRecord>(p => p.InitiatingStateEmailRecipientsModel.EmailTo == "Ea@Piipan.gov")), Times.Never);
        }

        [Fact]
        public async void Resolve_MatchCreationCheckWhenExistingRecord()
        {
            // Arrange
            var newId = "closedId";
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = "Api",
                SearchReason = null,
                MatchCreation = "Already Existing Match",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(newId);
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchDbo>()))
                .ReturnsAsync(new List<MatchDbo> {
                    new MatchDbo {
                        MatchId = "closedId",
                        CreatedAt = new DateTime(2020,01,02)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto()
            {
                Status = MatchRecordStatus.Open
            });

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateList);
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{newId}", firstMatch.MatchUrl);
            Assert.Equal(newId, firstMatch.MatchId);

            publishSearchMetrics.Verify(r => r.PublishSearchMetrics(
             It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                  r.Data.First().MatchCreation == search.MatchCreation &&
                                                  r.Data.First().SearchReason == search.SearchReason &&
                                                  r.Data.First().SearchFrom == search.SearchFrom)),
             Times.Once);

            notificationService.Verify(r => r.PublishNotificationOnMatchCreation(It.Is<NotificationRecord>(p => p.InitiatingStateEmailRecipientsModel.EmailTo == "Ea@Piipan.gov")), Times.Never);
        }
        [Fact]
        public async void Resolve_No_Notification_For_Not_Enabled_States()
        {
            // Arrange
            var record = new MatchDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = "Api",
                SearchReason = null,
                MatchCreation = "New Created Match",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchDetailsDto());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var stateInfoDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                stateInfoDao.Object,
                notificationService.Object
            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea", "Api", enabledStateShortList);

            // Assert
            recordApi.Verify(r => r.AddNewMatchRecord(
                It.Is<IMatchDbo>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Once);

            publishSearchMetrics.Verify(r => r.PublishSearchMetrics(
              It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                   r.Data.First().MatchCreation == search.MatchCreation &&
                                                   r.Data.First().SearchReason == search.SearchReason &&
                                                   r.Data.First().SearchFrom == search.SearchFrom)),
                                                   Times.Once);

            publishMatchMetrics.Verify(r => r.PublishMatchMetric(
             It.Is<ParticipantMatchMetrics>(r => r.MatchId == "foo" &&
                                                  r.InitState == record.Initiator
                                                 )),
                                                  Times.Once);
            // Need to be called only when creating new Match Record
            notificationService.Verify(r => r.PublishNotificationOnMatchCreation(It.Is<NotificationRecord>(p => p.MatchEmailDetails.InitState == "Echo Alpha" && p.InitiatingStateEmailRecipientsModel.EmailTo == "Ea@Piipan.gov" && p.MatchingStateEmailRecipientsModel.EmailTo == "Eb@Piipan.gov")), Times.Never);
        }

    }
}