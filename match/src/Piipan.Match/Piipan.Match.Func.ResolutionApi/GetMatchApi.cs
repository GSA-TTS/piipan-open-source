using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.States.Core.Service;

namespace Piipan.Match.Func.ResolutionApi
{

    /// <summary>
    /// Azure Function implementing Get Match endpoint for Match Resolution API
    /// </summary>
    public class GetMatchApi : BaseApi
    {
        private readonly IMatchDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;

        private readonly IMatchDetailsAggregator _matchResAggregator;
        private readonly IStateInfoService _stateInfoService;
        private readonly IMemoryCache _memoryCache;

        public const string StateInfoCacheName = "StateInfo";

        public GetMatchApi(
            IMatchDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchDetailsAggregator matchResAggregator,
            IStateInfoService stateInfoService,
            IMemoryCache memoryCache)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _stateInfoService = stateInfoService;
            _memoryCache = memoryCache;
        }

        [FunctionName("GetMatch")]
        public async Task<IActionResult> GetMatch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "matches/{matchId}")] HttpRequest req,
            string matchId,
            ILogger logger)
        {
            LogRequest(logger, req);
            try
            {
                matchId = matchId?.ToUpper();
                var match = _matchRecordDao.GetRecordByMatchId(matchId);
                var matchResEvents = _matchResEventDao.GetEventsByMatchId(matchId);
                await Task.WhenAll(match, matchResEvents);

                string requestLocation = req.Headers["X-Request-Location"];

                // We ignore state checks when the requestLocation is  the National Office. They are allowed to retrieve any matches.
                // For requests where the requestLocation is NOT the National Office, we have to check that the one of the following
                // conditions is true- Either
                // a) the requestLocation matches either the initiating or matching state.
                //  OR
                // b) the requestLocation is a Region that contains either the initiating or matching state
                if (requestLocation != "*") //check if the requestLocation is not the National Office
                {
                    //Since the requestLocation is NOT National Office... 

                    //Retrieve all state metadata from the database.
                    var states = await _memoryCache.GetOrCreateAsync(StateInfoCacheName, async (e) =>
                    {
                        return await _stateInfoService.GetDecryptedStates();
                    });

                    //Identify all states whose abbreviation matches the requestLocation or whose region matches the requestLocation
                    states = states.Where(n => string.Compare(n.StateAbbreviation, requestLocation, true) == 0
                        || string.Compare(n.Region, requestLocation, true) == 0);

                    // If the identified states associated with the requested location don't exist in the Match 
                    // record's states (i.e. the initiating or matching state), log an error and return a Not Found response
                    if (!match.Result.States.Any(s => states.Any(n => string.Compare(n.StateAbbreviation, s, true) == 0)))
                    {
                        logger.LogInformation("(NOTAUTHORIZEDMATCH) user {User} did not have access to match id {MatchId}", req.HttpContext?.User.Identity.Name, matchId);
                        return NotFoundErrorResponse(null);
                    }
                }

                var matchResRecord = _matchResAggregator.BuildAggregateMatchDetails(match.Result, matchResEvents.Result);
                var response = new MatchResApiResponse() { Data = matchResRecord };
                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogInformation(ex.Message);
                return NotFoundErrorResponse(ex);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }
    }
}
