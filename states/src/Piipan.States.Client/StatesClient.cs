using Piipan.Shared.Http;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.States.Client
{
    /// <summary>
    /// HTTP client for interacting with Piipan.States.Func.Api
    /// </summary>
    public class StatesClient : IStatesApi
    {
        private readonly IAuthorizedApiClient<StatesClient> _apiClient;

        /// <summary>
        /// Intializes a new instance of StatesClient
        /// </summary>
        /// <param name="apiClient">an API client instance scoped to StatesClient</param>
        public StatesClient(IAuthorizedApiClient<StatesClient> apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Sends a GET request to the /states endpoint using the API client's configured base URL.
        /// </summary>
        /// <returns></returns>
        public async Task<StatesInfoResponse> GetStates()
        {
            var states = await _apiClient.GetAsync<StatesInfoResponse>("states");
            return states;
        }

        /// <summary>
        /// Sends a POST request to the /insert_state endpoint using the API client's configured base URL.
        /// </summary>
        /// <returns></returns>
        public async Task<string> UpsertState(StateInfoRequest stateToInsert)
        {
            var response = await _apiClient.PostAsync<StateInfoRequest, string>("upsert_state", stateToInsert);
            return response;
        }
    }
}