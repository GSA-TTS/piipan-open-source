using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Piipan.Notification.Common;

namespace Piipan.Notifications.Common.Tests
{
    public class BasePageTest
    {
        protected ViewRenderService SetupRenderingApi()
        {
            var server = new PageTestServerFixture();
            var serviceProvider = server.GetRequiredService<IServiceProvider>();
            var viewEngine = server.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = server.GetRequiredService<ITempDataProvider>();

            var viewRenderer = new ViewRenderService(viewEngine, tempDataProvider, serviceProvider);
            return viewRenderer;
        }
    }
}
