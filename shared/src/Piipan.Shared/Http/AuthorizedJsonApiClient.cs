using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Piipan.Shared.Authentication;
using Polly;

namespace Piipan.Shared.Http
{
    /// <summary>
    /// Client for making authorized API calls within the Piipan system
    /// </summary>
    public class AuthorizedJsonApiClient<T> : IAuthorizedApiClient<T>
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITokenProvider<T> _tokenProvider;
        private const string _accept = "application/json";
        public int NoOfRetries { get; set; } = 3;
        public int RetryInterval { get; set; } = 5;
        private readonly HttpStatusCode[] retryResponseCodes = new HttpStatusCode[]
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };
        /// <summary>
        /// Creates a new instance of AuthorizedJsonApiClient
        /// </summary>
        /// <param name="clientFactory">an instance of IHttpClientFactory</param>
        /// <param name="tokenProvider">an instance of ITokenProvider</param>
        public AuthorizedJsonApiClient(IHttpClientFactory clientFactory,
            ITokenProvider<T> tokenProvider)
        {
            _clientFactory = clientFactory;
            _tokenProvider = tokenProvider;
        }

        private async Task<HttpRequestMessage> PrepareRequest(string path, HttpMethod method, IEnumerable<(string, string)> headerFactory = null)
        {
            var token = await _tokenProvider.RetrieveAsync();
            var httpRequestMessage = new HttpRequestMessage(method, path)
            {
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {token}" },
                    { HttpRequestHeader.Accept.ToString(), _accept }
                }
            };
            // add any additional headers using the supplied callback
            headerFactory?.ToList().ForEach(h => httpRequestMessage.Headers.Add(h.Item1, h.Item2));

            return httpRequestMessage;
        }
        private async Task<HttpResponseMessage> PrepareAndSendAsyncForGet(string path, IEnumerable<(string, string)> headerFactory = null)
        {


            var response = await Policy.HandleResult<HttpResponseMessage>(r => retryResponseCodes.Contains(r.StatusCode))
                    .WaitAndRetryAsync(NoOfRetries, retryAttempt => TimeSpan.FromSeconds(RetryInterval))
                    .ExecuteAsync(async () => await BaseSendAsync(await PrepareRequest(path, HttpMethod.Get, headerFactory)));
            return response;
        }
        private async Task<HttpResponseMessage> PrepareAndSendAsync<TRequest>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory, HttpMethod httpMethod)
        {
            var response = await Policy.HandleResult<HttpResponseMessage>(r => retryResponseCodes.Contains(r.StatusCode))
                    .WaitAndRetryAsync(NoOfRetries, retryAttempt => TimeSpan.FromSeconds(RetryInterval))
                    .ExecuteAsync(async () => await BaseSendAsync(await PrepareRequestWithBody(path, body, headerFactory, httpMethod)));
            return response;

        }

        private async Task<HttpRequestMessage> PrepareRequestWithBody<TRequest>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory, HttpMethod httpMethod)
        {
            var requestMessage = await PrepareRequest(path, httpMethod);

            // add any additional headers using the supplied callback
            headerFactory.Invoke().ToList().ForEach(h => requestMessage.Headers.Add(h.Item1, h.Item2));

            var json = JsonConvert.SerializeObject(body);

            requestMessage.Content = new StringContent(json);
            return requestMessage;
        }
        /// <summary>
        /// Do not call this function directly. This method is split out for testing purposes.
        /// </summary>
        protected virtual Task<HttpResponseMessage> BaseSendAsync(HttpRequestMessage request)
        {
            return Client().SendAsync(request);
        }
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body)
        {
            return await PostAsync<TRequest, TResponse>(path, body, () => Enumerable.Empty<(string, string)>());
        }
        /// <summary>
        /// Post a ASync call with Path and the body of the request
        /// </summary>
        /// <param name="path">path of the request</param>
        /// <param name="body">body of the request</param>
        /// <param name="headerFactory">header information</param>
        /// <returns></returns>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory)
        {
            HttpResponseMessage response = await PrepareAndSendAsync(path, body, headerFactory, HttpMethod.Post);
            response.EnsureSuccessStatusCode();
            var responseContentJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(responseContentJson);
        }
        /// <summary>
        /// Patch a ASync call with Path and the body of the request
        /// </summary>
        /// <param name="path">path of the request</param>
        /// <param name="body">body of the request</param>
        /// <param name="headerFactory">header information</param>
        /// <returns></returns>
        public async Task<(TResponse SuccessResponse, string FailResponse)> PatchAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory)
        {
            HttpResponseMessage response = await PrepareAndSendAsync(path, body, headerFactory, HttpMethod.Patch);

            var responseContentJson = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // If the response content is empty, at least make a new ApiHttpError with the status code.
                if (string.IsNullOrEmpty(responseContentJson))
                {
                    responseContentJson = JsonConvert.SerializeObject(new ApiHttpError() { Status = response.StatusCode.ToString() });
                }
                return (default, responseContentJson);
            }

            return (JsonConvert.DeserializeObject<TResponse>(responseContentJson), default);
        }
        /// <summary>
        /// GET  the response for an ASync call for the Request
        /// </summary>
        /// <param name="path">path of the request</param>
        /// <param name="TRequest">Request body</param>
        /// <returns></returns>
        public async Task<TResponse> GetAsync<TResponse, TRequest>(string path, TRequest requestObject) where TRequest : class, new()
        {
            return await GetAsync<TResponse>(path, QueryStringBuilder.ToQueryString(requestObject));
        }
        /// <summary>
        /// GET  the response for an ASync call for the Request
        /// </summary>
        /// <param name="path">path of the request</param>
        /// <param name="query">query for the request</param>
        /// <returns></returns>
        public async Task<TResponse> GetAsync<TResponse>(string path, string query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                path += query;
            }
            HttpResponseMessage response = await PrepareAndSendAsyncForGet(path);

            response.EnsureSuccessStatusCode();

            var responseContentJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseContentJson);
        }
        /// <summary>
        /// GET  the response for an ASync call for the Request.  Resopnse will come back as 404 if there any exceptions.
        /// </summary>
        /// <param name="path">path of the request</param>
        /// <param name="headerFactory">header information</param>
        /// <returns></returns>
        public async Task<(TResponse Response, int StatusCode)> TryGetAsync<TResponse>(string path, IEnumerable<(string, string)> headerFactory = null, string query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                path = $"{path}?{query}";
            }
            HttpResponseMessage response = await PrepareAndSendAsyncForGet(path, headerFactory);

            try
            {
                response.EnsureSuccessStatusCode();

                var responseContentJson = await response.Content.ReadAsStringAsync();

                return (JsonConvert.DeserializeObject<TResponse>(responseContentJson), (int)response.StatusCode);
            }
            catch (HttpRequestException)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (default, (int)response.StatusCode);
                }
                throw;
            }
        }

        private HttpClient Client()
        {
            var clientName = typeof(T).Name;
            return _clientFactory.CreateClient(clientName);
        }
    }
}
