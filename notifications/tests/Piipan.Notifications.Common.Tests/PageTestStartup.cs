using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Notifications.Common.Tests
{
    /// <summary>
    /// The startup class for rendering CSHTML pages
    /// </summary>
    public class PageTestStartup : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
