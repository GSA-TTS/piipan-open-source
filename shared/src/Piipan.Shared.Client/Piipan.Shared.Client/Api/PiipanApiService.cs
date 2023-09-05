using System.Net.Http.Json;
using System.Web;

namespace Piipan.Shared.Client.Api
{
    public abstract class PiipanApiService
    {
        protected PiipanApiService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }
        public HttpClient HttpClient { get; }

        private string ToQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }

        /// <summary>
        /// Safely attempts to get an object from the API. If an error is thrown,
        /// a new ApiResponse is created with that error encapsulated in it.
        /// </summary>
        /// <typeparam name="T">The type to be returned within the API response</typeparam>
        /// <param name="basePath">The API request base path (before adding query params)</param>
        /// <param name="requestObject">The object to add to the query string before calling the API</param>
        /// <returns></returns>
        public async Task<ApiResponse<T>> GetFromApi<T>(string basePath, object requestObject = null)
        {
            try
            {
                string fullPath = basePath;
                if (requestObject != null)
                {
                    fullPath += $"?{ToQueryString(requestObject)}";
                }
                return await HttpClient.GetFromJsonAsync<ApiResponse<T>>(fullPath);
            }
            catch
            {
                // TODO: Consider logging this error to Azure

                var apiResponse = new ApiResponse<T>();
                apiResponse.AddError("Error fetching data. Please try again later.");
                return apiResponse;
            }
        }

        /// <summary>
        /// Safely attempts to post an object to the API. If an error is thrown,
        /// a new ApiResponse is created with that error encapsulated in it.
        /// </summary>
        /// <typeparam name="TUpdate">The type to be sent in the body of the post request</typeparam>
        /// <typeparam name="TUpdate">The type to be returned inside the API Response as a result of the post request</typeparam>
        /// <param name="apiPath">The API request URL</param>
        /// <param name="objectToSave">The object to put in the body of the request</param>
        /// <returns></returns>
        public async Task<ApiResponse<TResponse>> PostToApi<TUpdate, TResponse>(string apiPath, TUpdate objectToSave)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(apiPath, objectToSave);
                return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
            }
            catch
            {
                // TODO: Consider logging this error to Azure

                var apiResponse = new ApiResponse<TResponse>();
                apiResponse.AddError("Error saving data. Please try again later.");
                return apiResponse;
            }
        }
    }
}
