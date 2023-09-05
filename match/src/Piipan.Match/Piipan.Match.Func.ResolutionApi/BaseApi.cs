using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Http;

namespace Piipan.Match.Func.ResolutionApi
{
    public class BaseApi
    {
        protected void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string subscription = request.Headers["Ocp-Apim-Subscription-Name"];
            if (subscription != null)
            {
                logger.LogInformation("Using APIM subscription {Subscription}", subscription);
            }

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
        }

        protected ActionResult NotFoundErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.NotFound),
                Title = "NotFoundException",
                Detail = "not found"
            });
            return (ActionResult)new NotFoundObjectResult(errResponse);
        }

        protected ActionResult InternalServerErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.InternalServerError),
                Title = ex.GetType().Name,
                Detail = ex.Message
            });
            return (ActionResult)new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
