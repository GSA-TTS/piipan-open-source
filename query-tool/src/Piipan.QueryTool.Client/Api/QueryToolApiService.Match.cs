using System.Threading.Tasks;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Client.Api;

namespace Piipan.QueryTool.Client.Api
{
    /// <summary>
    /// Extending the PiipanApiService
    /// </summary>
    public partial class QueryToolApiService : PiipanApiService, IQueryToolApiService
    {
        private const string MatchApiPath = "/api/match";

        /// <summary>
        /// The API call to get a match by ID
        /// </summary>
        /// <param name="matchId">The Match ID to be searched on</param>
        /// <returns>An ApiResponse that contains any errors or the found match data</returns>
        public async Task<ApiResponse<MatchResApiResponse>> GetMatchDetailById(string matchId)
        {
            return await GetFromApi<MatchResApiResponse>($"{MatchApiPath}/{matchId}");
        }

        /// <summary>
        /// The API call to get all matches by ID
        /// </summary>
        /// <returns>An ApiResponse that contains any errors or the found match data</returns>
        public async Task<ApiResponse<MatchResListApiResponse>> GetAllMatchDetails()
        {
            return await GetFromApi<MatchResListApiResponse>(MatchApiPath);
        }

        /// <summary>
        /// The API call to save an update to a match
        /// </summary>
        /// <returns>An ApiResponse that contains any errors or a boolean that says whether the save was successful or not</returns>
        public async Task<ApiResponse<MatchDetailSaveResponse>> SaveMatchUpdate(string matchId, DispositionModel dispositionModel)
        {
            return await PostToApi<DispositionModel, MatchDetailSaveResponse>($"{MatchApiPath}/{matchId}", dispositionModel);
        }
    }
}
