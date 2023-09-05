using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;
using System.Threading.Tasks;

namespace Piipan.Shared.Logging
{
    public class AuthenticationLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public const string CLAIMS_LOGGED_KEY = "CLAIMS_LOGGED";
        public const string USER_EMAIL_KEY = "USER_EMAIL";

        public AuthenticationLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<AuthenticationLoggingMiddleware> logger,
            IClaimsProvider claimsProvider)
        {
            if (!context.Session.GetInt32(CLAIMS_LOGGED_KEY).HasValue || context.Session.GetInt32(CLAIMS_LOGGED_KEY).Equals(0))
            {
                string loggedInUsersName = claimsProvider.GetEmail(context.User);
                logger.LogInformation($"User logged in: {loggedInUsersName}");
                foreach (var claim in context.User.Claims)
                {
                    logger.LogInformation($"[Session: {context.Session.Id}][CLAIM] {claim.Type}: {claim.Value}");
                }
            }

            if (context.Request != null && context.Request.Headers["Referer"].ToString() != null && context.Request.Headers["Referer"].ToString().Contains("logout"))
            {
                string loggedInUsersName = context.Session.GetString(USER_EMAIL_KEY);
                logger.LogInformation($"User logged out: {loggedInUsersName}");
                context.Session.SetInt32(CLAIMS_LOGGED_KEY, 0);
            }
            else
            {
                context.Session.SetInt32(CLAIMS_LOGGED_KEY, 1);
                string loggedInUsersName = claimsProvider.GetEmail(context.User);
                if (loggedInUsersName != null)
                {
                    context.Session.SetString(USER_EMAIL_KEY, loggedInUsersName);
                }
            }



            await _next(context);
        }
    }
}