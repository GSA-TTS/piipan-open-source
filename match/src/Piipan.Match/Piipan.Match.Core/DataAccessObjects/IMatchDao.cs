using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchDao
    {
        Task<string> AddRecord(MatchDbo record);
        Task<IEnumerable<IMatchDbo>> GetRecordsByHashAndState(MatchDbo record);
        Task<IMatchDbo> GetRecordByMatchId(string matchId);
        Task<IEnumerable<IMatchDbo>> GetMatchesById(string[] matchIds);
        Task<IEnumerable<IMatchDbo>> GetMatches();
    }
}
