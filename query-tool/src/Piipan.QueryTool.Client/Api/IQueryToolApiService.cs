using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Client.Api;

namespace Piipan.QueryTool.Client.Api
{
    public interface IQueryToolApiService
    {
        Task<ApiResponse<OrchMatchResponseData>> SubmitDuplicateParticipantSearchRequest(DuplicateParticipantQuery query);
        Task<ApiResponse<MatchResApiResponse>> GetMatchDetailById(string matchId);
        Task<ApiResponse<MatchResListApiResponse>> GetAllMatchDetails();
        Task<ApiResponse<MatchDetailSaveResponse>> SaveMatchUpdate(string matchId, DispositionModel dispositionModel);
    }
}
