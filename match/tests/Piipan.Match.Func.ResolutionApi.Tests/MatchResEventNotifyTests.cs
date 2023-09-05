using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class MatchResEventNotifyTests
    {
        private MatchDetailsDto BeforeMatchUpdate = new MatchDetailsDto()
        {
            MatchId = "foo",
            Initiator = "ea",
            States = new string[] { "ea", "eb" },
            Dispositions = new Disposition[]
            {
                new Disposition
                {
                    State = "ea"
                },
                new Disposition
                {
                    State = "eb"
                }
            },
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open"
        };

        private MatchDetailsDto AfterMatchUpdate = new MatchDetailsDto()
        {
            MatchId = "foo",
            Initiator = "ea",
            States = new string[] { "ea", "eb" },
            Dispositions = new Disposition[]
            {
                new Disposition
                {
                    State = "ea"
                },
                new Disposition
                {
                    State = "eb"
                }
            },
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open"
        };

        [Fact]
        public async Task MatchResEventNotify_OnlyLogsWhenNoEventsToNotify()
        {
            var logger = new Mock<ILogger<MatchResEventNotify>>();
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var matchResNotifyService = new Mock<IMatchResNotifyService>();

            matchResEventDao.Setup(n => n.GetEventsNotNotified()).ReturnsAsync(Array.Empty<IMatchResEvent>());

            MatchResEventNotify matchResEventNotify = new MatchResEventNotify(
                logger.Object,
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                matchResNotifyService.Object);
            await matchResEventNotify.Run(new Microsoft.Azure.WebJobs.TimerInfo(null, null, true));

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MatchResEventNotify trigger function executed at:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MatchResEventNotify Timer is running late!")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
            matchRecordDao.Verify(n => n.GetMatchesById(It.IsAny<string[]>()), Times.Never);
            matchResAggregator.Verify(n => n.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()), Times.Never);
            matchResNotifyService.Verify(n => n.SendNotification(It.IsAny<MatchDetailsDto>(), It.IsAny<MatchDetailsDto>()), Times.Never);
            matchResEventDao.Verify(n => n.UpdateMatchRecordsNotifiedAt(It.IsAny<int[]>()), Times.Never);
        }

        [Fact]
        public async Task MatchResEventNotify_DoesNotSendsNotifications_WhenMatchDoesNotExist_ButStillUpdatesToNotified()
        {
            var logger = new Mock<ILogger<MatchResEventNotify>>();
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var matchResNotifyService = new Mock<IMatchResNotifyService>();

            matchResEventDao.Setup(n => n.GetEventsNotNotified()).ReturnsAsync(new IMatchResEvent[] { new MatchResEventDbo { Id = 1, MatchId = "123" } });
            matchRecordDao.Setup(n => n.GetMatchesById(It.IsAny<string[]>())).ReturnsAsync(Array.Empty<IMatchDbo>());

            MatchResEventNotify matchResEventNotify = new MatchResEventNotify(
                logger.Object,
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                matchResNotifyService.Object);
            await matchResEventNotify.Run(new Microsoft.Azure.WebJobs.TimerInfo(null, null, false));

            matchRecordDao.Verify(n => n.GetMatchesById(It.IsAny<string[]>()), Times.Once);
            matchResAggregator.Verify(n => n.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()), Times.Never);
            matchResNotifyService.Verify(n => n.SendNotification(It.IsAny<MatchDetailsDto>(), It.IsAny<MatchDetailsDto>()), Times.Never);
            matchResEventDao.Verify(n => n.UpdateMatchRecordsNotifiedAt(It.Is<int[]>(i => i.Length == 1 && i[0] == 1)), Times.Once);
        }

        [Fact]
        public async Task MatchResEventNotify_SendsNotifications_WhenMatchExists_AndUpdatesToNotified()
        {
            var logger = new Mock<ILogger<MatchResEventNotify>>();
            var matchRecordDao = new Mock<IMatchDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchDetailsAggregator>();
            var matchResNotifyService = new Mock<IMatchResNotifyService>();

            matchResEventDao.Setup(n => n.GetEventsNotNotified()).ReturnsAsync(
                new IMatchResEvent[]
                {
                    new MatchResEventDbo { Id = 1, MatchId = "123", Delta = "{ \"vulnerable_individual\": true }", NotifiedAt = DateTime.Now.AddMinutes(-60) },
                    new MatchResEventDbo { Id = 2, MatchId = "123", Delta = "{ \"vulnerable_individual\": false }" },
                });
            matchRecordDao.Setup(n => n.GetMatchesById(It.IsAny<string[]>())).ReturnsAsync(
                new IMatchDbo[]
                {
                    new MatchDbo
                    {
                        States = new string[] { "ea", "eb" },
                        MatchId = "123"
                    }
                });
            var beforeUpdateMatchRecord = new MatchDetailsDto
            {
                MatchId = "123",
                Dispositions = new Disposition[]
                {
                    new Disposition
                    {
                        VulnerableIndividual = true
                    }
                }
            };
            var afterUpdateMatchRecord = new MatchDetailsDto
            {
                MatchId = "123",
                Dispositions = new Disposition[]
                {
                    new Disposition
                    {
                        VulnerableIndividual = false
                    }
                }
            };
            matchResAggregator.SetupSequence(n => n.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(beforeUpdateMatchRecord)
                .Returns(afterUpdateMatchRecord);

            MatchResEventNotify matchResEventNotify = new MatchResEventNotify(
                logger.Object,
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                matchResNotifyService.Object);
            await matchResEventNotify.Run(new Microsoft.Azure.WebJobs.TimerInfo(null, null, false));

            matchRecordDao.Verify(n => n.GetMatchesById(It.IsAny<string[]>()), Times.Once);
            matchResAggregator.Verify(n => n.BuildAggregateMatchDetails(It.IsAny<IMatchDbo>(), It.IsAny<IEnumerable<IMatchResEvent>>()), Times.Exactly(2));
            matchResNotifyService.Verify(n => n.SendNotification(beforeUpdateMatchRecord, afterUpdateMatchRecord), Times.Once);
            matchResEventDao.Verify(n => n.UpdateMatchRecordsNotifiedAt(It.Is<int[]>(i => i.Length == 2 && i[0] == 1 && i[1] == 2)), Times.Once);
        }
    }
}
