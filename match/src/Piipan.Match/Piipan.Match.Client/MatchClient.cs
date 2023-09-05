using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Shared.Http;

namespace Piipan.Match.Client
{
    /// <summary>
    /// HTTP client for interacting with Piipan.Match.Func.Api
    /// </summary>
    public class MatchClient : IMatchSearchApi
    {
        private readonly IAuthorizedApiClient<MatchClient> _apiClient;

        /// <summary>
        /// Intializes a new instance of MatchClient
        /// </summary>
        /// <param name="apiClient">an API client instance scoped to MatchClient</param>
        public MatchClient(IAuthorizedApiClient<MatchClient> apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Sends a POST request to the /find_matches endpoint using the API client's configured base URL.
        /// Includes in the request headers the identifier of the state initiating the request
        /// </summary>
        /// <param name="request">A collection of participants to attempt to find matches for</param>
        /// <param name="initiatingState">The two character identifier of the state initiating the request</param>
        /// <returns></returns>
        public async Task<OrchMatchResponse> FindAllMatches(OrchMatchRequest request, string initiatingState)
        {
            return await _apiClient
                .PostAsync<OrchMatchRequest, OrchMatchResponse>("find_matches", request, () => 
                {
                    return new List<(string, string)>
                    {
                        ("X-Initiating-State", initiatingState)
                    };
                });
        }
    }
}
