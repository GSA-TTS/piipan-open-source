using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Notification.Common;

namespace Piipan.Notification.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterNotificationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRazorViewEngine, RazorViewEngine>();
            serviceCollection.AddTransient<ITempDataProvider, SessionStateTempDataProvider>();
            serviceCollection.AddTransient<IServiceProvider, ServiceProvider>();
            serviceCollection.AddTransient<IViewRenderService, ViewRenderService>();
        }

    }
}
