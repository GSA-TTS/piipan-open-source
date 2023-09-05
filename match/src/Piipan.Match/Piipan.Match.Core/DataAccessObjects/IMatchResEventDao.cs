using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchResEventDao
    {
        Task<int> AddEvent(MatchResEventDbo record);
        Task<IEnumerable<IMatchResEvent>> GetEventsByMatchId(
            string matchId,
            bool sortByAsc = true
        );

        Task<IEnumerable<IMatchResEvent>> GetEventsByMatchIDs(
            IEnumerable<string> matchIds,
            bool sortByAsc = true
        );

        Task<IEnumerable<IMatchResEvent>> GetEventsNotNotified();

        Task<int> UpdateMatchRecordsNotifiedAt(int[] ids);
    }
}
