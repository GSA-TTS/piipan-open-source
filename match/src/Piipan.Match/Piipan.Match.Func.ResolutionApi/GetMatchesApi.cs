using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Match.Func.ResolutionApi
{

    /// <summary>
    /// Azure Function implementing Get Match endpoint for Match Resolution API
    /// </summary>
    public class GetMatchesApi : BaseApi
    {
        private readonly IMatchDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;

        private readonly IMatchDetailsAggregator _matchResAggregator;

        public GetMatchesApi(
            IMatchDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchDetailsAggregator matchResAggregator)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
        }

        [FunctionName("GetMatches")]
        public async Task<IActionResult> GetMatches(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "matches")] HttpRequest req,
            ILogger logger)
        {
            LogRequest(logger, req);
            try
            {
                var matches = await _matchRecordDao.GetMatches();
                var matchResEvents = await _matchResEventDao.GetEventsByMatchIDs(matches.Select(n => n.MatchId));
                var matchRecords = matches.Select(n => _matchResAggregator.BuildAggregateMatchDetails(n, matchResEvents.Where(m => m.MatchId == n.MatchId)));
                var response = new MatchResListApiResponse() { Data = matchRecords };
                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }
    }
}
