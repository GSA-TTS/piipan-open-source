using System.Net;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Polly;

namespace Piipan.Shared.Client.Api
{
    public class PiipanHttpMessageHandler : DelegatingHandler
    {
        public PiipanHttpMessageHandler(PiipanNavigationManager navigationManager, IModalManager modalManager)
        {
            NavigationManager = navigationManager;
            ModalManager = modalManager;
        }

        public PiipanNavigationManager NavigationManager { get; set; }
        public IModalManager ModalManager { get; set; }

        /// <summary>
        /// Do not call this function directly. This method is split out for testing purposes.
        /// </summary>
        protected virtual Task<HttpResponseMessage> BaseSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryResponseCodes = new HttpStatusCode[]
            {
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.TooManyRequests,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout
            };
            var response = await Policy.HandleResult<HttpResponseMessage>(r => retryResponseCodes.Contains(r.StatusCode))
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(5))
                    .ExecuteAsync(async () => await BaseSendAsync(request, cancellationToken));

            // If a Forbidden or Unauthorized response was returned, refresh the page. We need an authorization token.
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }

            // If an HTML response was returned, that means we need to refresh our token. Refresh the page.
            if (response.Content.Headers.ContentType?.MediaType?.ToLower() == "text/html")
            {
                var htmlContent = await response.Content.ReadAsStringAsync();
                var searchString = "window.location.replace('";
                var indexOfRedirect = htmlContent.IndexOf(searchString);
                if (indexOfRedirect != -1)
                {
                    indexOfRedirect += searchString.Length;
                    var redirectLocation = htmlContent.Substring(indexOfRedirect, htmlContent.IndexOf('\'', indexOfRedirect) - indexOfRedirect);
                    ModalManager.Show(new RedirectToLoginModal()
                    {
                        RedirectLocation = redirectLocation
                    },
                    new ModalInfo()
                    {
                        ForceAction = true
                    });
                }
                else
                {
                    NavigationManager.NavigateTo(NavigationManager.Uri, true);
                }
            }

            return response;
        }
    }
}
