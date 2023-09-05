using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piipan.Shared.Http
{
    public interface IAuthorizedApiClient<T>
    {
        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        /// <param name="body">object to be sent as request body</param>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body);

        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        /// <param name="body">object to be sent as request body</param>
        /// <param name="headerFactory">callback which supplies additional headers to be included in the outbound request</param>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory);

        /// <summary>
        /// Send an asynchronous PATCH request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        /// <param name="body">object to be sent as request body</param>
        /// <param name="headerFactory">callback which supplies additional headers to be included in the outbound request</param>
        Task<(TResponse SuccessResponse, string FailResponse)> PatchAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory);

        /// <summary>
        /// Send an asynchronous GET request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        Task<TResponse> GetAsync<TResponse, TRequest>(string path, TRequest requestObject) where TRequest : class, new();

        /// <summary>
        /// Send an asynchronous GET request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        Task<TResponse> GetAsync<TResponse>(string path, string query = null);

        /// <summary>
        /// Attempts to send an asynchronous GET request to an API endpoint. If a 404 is not found, 
        /// it will return the default TResponse. Will return the status code whether a 404 or a success.
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        Task<(TResponse Response, int StatusCode)> TryGetAsync<TResponse>(string path, IEnumerable<(string, string)> headerFactory = null, string query = null);
    }
}