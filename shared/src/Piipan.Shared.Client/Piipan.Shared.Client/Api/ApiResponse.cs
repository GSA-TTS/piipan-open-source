namespace Piipan.Shared.Client.Api
{
    /// <summary>
    /// The response that should be returned by all web application APIs.
    /// Use this class to return an API response when the API needs to return a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// The item being retrieved or otherwise returned by the API
        /// </summary>
        public T? Value { get; set; }
    }

    /// <summary>
    /// The base response that should be returned by all web application APIs.
    /// Use this base class to return an API response when all you care about is whether the request succeeded or failed.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Any server errors that occurred while executing the API
        /// </summary>
        public List<ServerError> Errors { get; set; } = new List<ServerError>();

        /// <summary>
        /// True if the user is not authorized to execute this API request.
        /// </summary>
        public bool IsUnauthorized { get; set; } = false;

        public void AddError(string error, string propertyName = "")
        {
            Errors.Add(new ServerError(propertyName, error));
        }
    }
}
