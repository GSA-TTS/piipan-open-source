using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Shared.Tests.ViewRendering
{
    /// <summary>
    /// A test fixture for rendering CSHTML pages
    /// </summary>
    public class PageTestServerFixture : WebApplicationFactory<PageTestStartup>
    {
        private readonly string contentRootPath;
        private readonly string assembly;

        public PageTestServerFixture(string contentRootPath, string assembly) : base()
        {
            this.contentRootPath = contentRootPath;
            this.assembly = assembly;
        }
        public TService GetRequiredService<TService>()
        {
            if (Server == null)
            {
                CreateDefaultClient();
            }

            return Server.Host.Services.GetRequiredService<TService>();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder();
            // Solution fixing the problem:
            // https://github.com/dotnet/aspnetcore/issues/17655#issuecomment-581418168
            hostBuilder.ConfigureAppConfiguration((context, b) =>
            {
                context.HostingEnvironment.ApplicationName = assembly;
            });

            return hostBuilder.UseStartup<PageTestStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(contentRootPath);
        }
    }
}
