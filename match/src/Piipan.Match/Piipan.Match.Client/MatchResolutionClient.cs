using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.Http;

namespace Piipan.Match.Client
{
    /// <summary>
    /// HTTP client for interacting with Piipan.Match.Func.Api
    /// </summary>
    public class MatchResolutionClient : IMatchResolutionApi
    {
        private readonly IAuthorizedApiClient<MatchResolutionClient> _apiClient;

        /// <summary>
        /// Intializes a new instance of MatchClient
        /// </summary>
        /// <param name="apiClient">an API client instance scoped to MatchClient</param>
        public MatchResolutionClient(IAuthorizedApiClient<MatchResolutionClient> apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Sends a POST request to the /find_matches endpoint using the API client's configured base URL.
        /// Includes in the request headers the identifier of the state initiating the request
        /// </summary>
        /// <param name="request">A collection of participants to attempt to find matches for</param>
        /// <param name="requestLocation">The location (state, region, or national office) of the requestor</param>
        /// <returns></returns>
        public async Task<MatchResApiResponse> GetMatch(string matchId, string requestLocation)
        {
            var (response, _) = await _apiClient.TryGetAsync<MatchResApiResponse>($"matches/{matchId}", new List<(string, string)>
                    {
                        ("X-Request-Location", requestLocation)
                    });
            return response;
        }

        /// <summary>
        /// Sends a GET request to the /list endpoint using the API client's configured base URL.
        /// </summary>
        public async Task<MatchResListApiResponse> GetMatches()
        {
            var (response, _) = await _apiClient.TryGetAsync<MatchResListApiResponse>($"matches");
            return response;
        }

        /// <summary>
        /// sendsS a patch request to update match res events
        /// </summary>
        public async Task<(MatchResApiResponse SuccessResponse, string FailResponse)> AddMatchResEvent(string matchId, AddEventRequest request, string initiatingState)
        {
            return await _apiClient.PatchAsync<AddEventRequest, MatchResApiResponse>($"matches/{matchId}/disposition", request, () =>
            {
                return new List<(string, string)>
                    {
                        ("X-Initiating-State", initiatingState)
                    };
            });
        }
    }
}
