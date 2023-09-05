using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging.AzureAppServices;

namespace Piipan.QueryTool.Cypress.Tests
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() =>
            {
                CreateHostBuilder(args, "mock_eligibility_worker_user.json", "https://localhost:5001").Build().Run();
            }));
            tasks.Add(Task.Run(() =>
            {
                CreateHostBuilder(args, "mock_program_oversight_user.json", "https://localhost:5002").Build().Run();
            }));
            await Task.WhenAll(tasks);
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string mockUser, string urls) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddAzureWebAppDiagnostics())
                    .ConfigureServices(serviceCollection => serviceCollection
                        .Configure<AzureFileLoggerOptions>(options =>
                        {
                            options.FileName = "azure-diagnostics-";
                            options.FileSizeLimit = 50 * 1024;
                            options.RetainedFileCountLimit = 5;
                        }).Configure<AzureBlobLoggerOptions>(options =>
                        {
                            options.BlobName = "log.txt";
                        })
                    )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(urls);
                    webBuilder.UseStartup<Startup>(builderContext =>
                    {
                        return new Startup(builderContext.Configuration, builderContext.HostingEnvironment, mockUser);
                    });
                });
    }
}
