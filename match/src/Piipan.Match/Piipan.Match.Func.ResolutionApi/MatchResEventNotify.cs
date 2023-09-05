using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Services;

namespace Piipan.Match.Func.ResolutionApi
{
    public class MatchResEventNotify
    {
        private readonly IMatchDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;
        private readonly IMatchDetailsAggregator _matchResAggregator;
        private readonly IMatchResNotifyService _matchResNotifyService;
        private readonly ILogger<MatchResEventNotify> _log;


        public MatchResEventNotify(
            ILogger<MatchResEventNotify> log,
            IMatchDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchDetailsAggregator matchResAggregator,
            IMatchResNotifyService matchResNotifyService)
        {
            _log = log;
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _matchResNotifyService = matchResNotifyService;
        }

        [FunctionName("MatchResEventNotify")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
        {
            _log.LogInformation($"MatchResEventNotify trigger function executed at: {DateTime.Now}");
            if (myTimer.IsPastDue)
            {
                _log.LogInformation("MatchResEventNotify Timer is running late!");
            }

            var matchResEvents = await _matchResEventDao.GetEventsNotNotified();
            var totalRecords = matchResEvents.Count();

            if (totalRecords == 0)
            {
                return;
            }

            var matches = await _matchRecordDao.GetMatchesById(matchResEvents.Select(n => n.MatchId).Distinct().ToArray());
            foreach (var match in matches)
            {
                var oldMatchRecords = _matchResAggregator.BuildAggregateMatchDetails(match, matchResEvents.Where(m => m.MatchId == match.MatchId && m.NotifiedAt != null));
                var newMatchRecords = _matchResAggregator.BuildAggregateMatchDetails(match, matchResEvents.Where(m => m.MatchId == match.MatchId));
                await _matchResNotifyService.SendNotification(oldMatchRecords, newMatchRecords);
            }

            var recordsUpdated = await _matchResEventDao.UpdateMatchRecordsNotifiedAt(matchResEvents.Select(n => n.Id).ToArray());

            if (recordsUpdated != totalRecords)
            {
                _log.LogError($"Attempted to update {totalRecords} notified_at rows, but updated {recordsUpdated}.");
            }
        }
    }
}
