using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;

namespace Piipan.Shared.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<RequestLoggingMiddleware> logger,
            IClaimsProvider claimsProvider)
        {
            logger.LogInformation($"Request {context.TraceIdentifier}: {claimsProvider.GetEmail(context.User)} {context.Request.Method} {context.Request.Path}");
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Request {context.TraceIdentifier} Exception: {ex.Message}");
                throw;
            }
        }
    }
}