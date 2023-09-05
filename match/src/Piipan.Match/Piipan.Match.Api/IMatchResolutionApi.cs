using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.Match.Api
{
    public interface IMatchResolutionApi
    {
        Task<MatchResApiResponse> GetMatch(string matchId, string requestLocation);
        Task<MatchResListApiResponse> GetMatches();
        Task<(MatchResApiResponse SuccessResponse, string FailResponse)> AddMatchResEvent(string matchId, AddEventRequest request, string initiatingState);
    }
}
