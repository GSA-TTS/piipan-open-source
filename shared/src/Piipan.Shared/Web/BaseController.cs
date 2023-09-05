using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Roles;

namespace Piipan.Shared.Web
{
    /// <summary>
    /// This is a base controller class that sets up common interfaces.
    /// Any additional common logic between controllers can be placed here.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseController<T> : ControllerBase
    {
        protected IWebAppDataServiceProvider _webAppDataServiceProvider;
        protected IRolesProvider _rolesProvider;
        protected ILogger<T> _logger;
        protected ApiResponse ApiResponseContext { get; private set; }
        private const string DefaultErrorText = "Internal Server Error. Please contact system maintainers.";

        private readonly Dictionary<Type, ExceptionHandlerData<Exception>> exceptionHandlers = new();

        /// <summary>
        /// Initialize the logger and WebAppDataServiceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected BaseController(IServiceProvider serviceProvider)
        {
            _webAppDataServiceProvider = serviceProvider.GetRequiredService<IWebAppDataServiceProvider>();
            _rolesProvider = serviceProvider.GetRequiredService<IRolesProvider>();
            _logger = serviceProvider.GetRequiredService<ILogger<T>>();
        }

        protected bool UserHasRole(string area)
        {
            return _rolesProvider.TryGetRoles(area).Contains(_webAppDataServiceProvider.Role);
        }
        protected List<(string Property, string ErrorMessage)> GetModelStateErrors(ModelStateDictionary modelState, string propertyBasePath)
        {
            List<(string Property, string ErrorMessage)> errorList = new();
            var keys = modelState.Keys;
            foreach (var key in keys)
            {
                if (modelState[key]?.Errors?.Count > 0)
                {
                    var error = modelState[key].Errors[0];
                    errorList.Add(new($"{propertyBasePath}." + key, error.ErrorMessage));
                }
            }
            return errorList;
        }

        /// <summary>
        /// Register an exception handler. Any time these exceptions are thrown, the exceptionHandler task will fire off instead of the generic exception handler.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exceptionHandler"></param>
        /// <param name="statusCode"></param>
        protected void RegisterExceptionHandler<TException>(ExceptionHandlerData<TException> exceptionHandlerData) where TException : Exception
        {
            Type exceptionType = typeof(TException);
            if (exceptionHandlers.ContainsKey(exceptionType))
            {
                _logger.LogInformation($"Exception handler for type {exceptionType.Name} not applied. Exception already registered.");
            }
            else
            {
                var wrapperHandlerData = new ExceptionHandlerData<Exception>
                {
                    ExceptionHandlerCallback = exceptionHandlerData.ExceptionHandlerCallback == null ? null :
                        (Exception ex) => exceptionHandlerData.ExceptionHandlerCallback.Invoke(ex as TException),
                    StatusCode = exceptionHandlerData.StatusCode,
                    AssociatedProperty = exceptionHandlerData.AssociatedProperty,
                    DisplayToUserMessage = exceptionHandlerData.DisplayToUserMessage
                };
                exceptionHandlers.Add(exceptionType, wrapperHandlerData);
            }
        }

        /// <summary>
        /// Call HandleException when you want to handle an exception immediately.
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
        /// <param name="displayToUserMessage">The message to display to the user</param>
        /// <param name="associatedProperty">The property associated with this exception, if any</param>
        /// <param name="statusCode">The status code that should be returned from the API</param>
        protected void HandleException(Exception exception, string displayToUserMessage = null, string associatedProperty = "", HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, exception.Message);
            ApiResponseContext.AddError(displayToUserMessage ?? DefaultErrorText, associatedProperty);
            Response.StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Creates a basic API response, safely executes the callback function, and returns the result.
        /// </summary>
        /// <param name="func">The function to be executed within the ApiResponseContext</param>
        /// <param name="defaultErrorMessage">The default error message should an exception occur during processing</param>
        /// <returns>The generated ApiResponse</returns>
        protected async Task<ApiResponse> CreateApiResponse(Func<ApiResponse, Task> func, string defaultErrorMessage = null)
        {
            ApiResponseContext = new ApiResponse();
            await ExecuteApiFunction(func, defaultErrorMessage);
            return ApiResponseContext;
        }


        /// <summary>
        /// Creates an API response that returns a type of TResponse, safely executes the callback function, and returns the result.
        /// </summary>
        /// <param name="func">The function to be executed within the ApiResponseContext</param>
        /// <param name="defaultErrorMessage">The default error message should an exception occur during processing</param>
        /// <returns>The generated ApiResponse</returns>
        protected async Task<ApiResponse<TResponse>> CreateApiResponse<TResponse>(Func<ApiResponse<TResponse>, Task> func, string defaultErrorMessage = null)
        {
            ApiResponseContext = new ApiResponse<TResponse>();
            await ExecuteApiFunction(func, defaultErrorMessage);
            return ApiResponseContext as ApiResponse<TResponse>;

        }

        private async Task ExecuteApiFunction<TResponse>(Func<TResponse, Task> func, string defaultErrorMessage = null) where TResponse : ApiResponse
        {
            try
            {
                await _webAppDataServiceProvider.Initialize();
                await func.Invoke(ApiResponseContext as TResponse);
            }
            catch (Exception ex)
            {
                if (exceptionHandlers.TryGetValue(ex.GetType(), out var handlerData))
                {
                    _logger.LogError(ex, ex.Message);
                    if (handlerData.ExceptionHandlerCallback != null)
                    {
                        await handlerData.ExceptionHandlerCallback(ex);
                    }
                    else
                    {
                        ApiResponseContext.AddError(handlerData.DisplayToUserMessage ?? DefaultErrorText, handlerData.AssociatedProperty ?? "");
                    }
                    Response.StatusCode = (int)handlerData.StatusCode;
                }
                else
                {
                    HandleException(ex, defaultErrorMessage);
                }
            }
        }
    }
}
