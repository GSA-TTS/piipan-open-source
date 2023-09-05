using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Tests.Mocks;
using Piipan.Shared.Web;

namespace Piipan.Shared.Tests.ViewRendering
{
    /// <summary>
    /// The startup class for rendering CSHTML pages
    /// </summary>
    public class PageTestStartup : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var webAppDataServiceProviderMock = DefaultMocks.IWebAppDataServiceProviderMock();
            services.AddTransient<IWebAppDataServiceProvider>((provider) => webAppDataServiceProviderMock.Object);
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
