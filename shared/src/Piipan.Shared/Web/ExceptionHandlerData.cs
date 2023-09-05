using System;
using System.Net;
using System.Threading.Tasks;

namespace Piipan.Shared.Web
{
    /// <summary>
    /// This class keeps track of the status code, assocated property, and display messages of exceptions thrown on the server side ASP.NET code.
    /// If any special exception logic needs to happen, the ExceptionHandlerCallback can be filled in.
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    public class ExceptionHandlerData<TException> where TException : Exception
    {
        public Func<TException, Task> ExceptionHandlerCallback { get; init; }

        // If we're handling an expected exception, such as an ArgumentException, we shouldn't default to 500.
        // 500 will cause Blazor to retry, and we don't want to do that by default.
        public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
        public string AssociatedProperty { get; init; }
        public string DisplayToUserMessage { get; init; }
    }
}
