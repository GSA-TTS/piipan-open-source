using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Moq;
using NEasyAuthMiddleware;
using Piipan.Match.Api;
using Piipan.Match.Client;
using Piipan.Match.Func.Api;
using Piipan.Match.Func.Api.IntegrationTests;
using Piipan.Match.Func.ResolutionApi;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;
using Piipan.Shared.Tests.TestFixtures;
using Piipan.States.Api;
using Piipan.States.Client;
using Piipan.States.Func.Api;
using Piipan.States.Func.Api.IntegrationTests;

namespace Piipan.QueryTool.Cypress.Tests
{
    [ExcludeFromCodeCoverage]
    public class Startup : Piipan.QueryTool.Startup
    {
        private static PiipanAzureFunctionServer? MatchTestServer = null;
        private static PiipanAzureFunctionServer? MatchResTestServer = null;
        private static PiipanAzureFunctionServer? StatesTestServer = null;
        private static SemaphoreSlim ServerLocker = new SemaphoreSlim(1);
        private static bool DatabaseInsertsCompleted = false;
        private string _mockFile;
        private Mock<IHttpClientFactory> _httpClientFactory = new();
        public Startup(IConfiguration configuration, IWebHostEnvironment env, string mockFile) : base(configuration, env)
        {
            _mockFile = mockFile;

            // Needed so .sql files look in the bin directory instead of base folder.
            Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6));
        }

        protected override void UseMockFile(IServiceCollection services)
        {
            var mockFile = $"{_env.ContentRootPath}/{_mockFile}";
            services.UseJsonFileToMockEasyAuth(mockFile);
        }

        protected override void RegisterMatchClientServices(IServiceCollection services)
        {
            var clientFactory = services.FirstOrDefault(n => n.ServiceType == typeof(IHttpClientFactory));
            services.Remove(clientFactory);
            RegisterApi<IMatchSearchApi, MatchClient, BaseMatchIntegrationTests>
                (services, ref MatchTestServer, 1, typeof(MatchApi));
        }

        protected override void RegisterMatchResolutionClientServices(IServiceCollection services)
        {
            RegisterApi<IMatchResolutionApi, MatchResolutionClient, Piipan.Match.Func.ResolutionApi.IntegrationTests.BaseMatchResIntegrationTests>
                (services, ref MatchResTestServer, 2, typeof(GetMatchApi), typeof(GetMatchesApi), typeof(AddEventApi));
        }

        protected override void RegisterStatesClientServices(IServiceCollection services)
        {
            RegisterApi<IStatesApi, StatesClient, BaseStateIntegrationTest>
                (services, ref StatesTestServer, 3, typeof(StateApi), typeof(UpsertState));
        }

        protected override void AddStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.WebRootFileProvider is CompositeFileProvider compositeFileProvider)
            {
                // Grab the files from Query Tool project so that we don't need to duplicate them in ours
                var queryToolProvider = new PhysicalFileProvider(env.ContentRootPath.Replace("tests\\Piipan.QueryTool.Cypress.Tests", "src\\Piipan.QueryTool\\wwwroot"));
                var currentProviders = compositeFileProvider.FileProviders.ToList();
                currentProviders.Add(queryToolProvider);

                env.WebRootFileProvider = new CompositeFileProvider(currentProviders);
            }

            app.UseStaticFiles();
        }

        protected override void FinishConfiguringServices(IApplicationBuilder applicationBuilder)
        {
            ServerLocker.Wait();
            try
            {
                if (!DatabaseInsertsCompleted)
                {
                    DatabaseInsertsCompleted = true;
                    var iTests = MatchTestServer!.IntegrationTest as BaseMatchIntegrationTests;
                    iTests!.Insert(iTests.GetEncryptedFullRecord(iTests.GetDefaultRecord()));

                    var statesApi = applicationBuilder.ApplicationServices.GetRequiredService<IStatesApi>();
                    _ = (statesApi.UpsertState(new States.Api.Models.StateInfoRequest
                    {
                        Data =
                        {
                            Id = "101",
                            Email = "test-ea@example.com",
                            State = "Echo Alpha",
                            StateAbbreviation = "EA",
                            Region = "EA-MPRO"
                        }
                    })).Result;
                    _ = (statesApi.UpsertState(new States.Api.Models.StateInfoRequest
                    {
                        Data =
                        {
                            Id = "102",
                            Email = "test-eb@example.com",
                            State = "Echo Bravo",
                            StateAbbreviation = "EB",
                            Region = "EB-MPRO"
                        }
                    })).Result;
                }
            }
            finally
            {
                ServerLocker.Release();
            }
        }

        private void RegisterApi<TService, TImplementation, TIntegrationTest>
            (IServiceCollection services, ref PiipanAzureFunctionServer piipanTestServer, int port, params Type[] types)
            where TService : class
            where TImplementation : class, TService
            where TIntegrationTest : class, IIntegrationTest, new()
        {
            ServerLocker.Wait();
            try
            {
                if (piipanTestServer == null)
                {
                    piipanTestServer = PiipanAzureFunctionServer.CreateWithPort<TIntegrationTest>(port.ToString(), types);
                }
            }
            finally
            {
                // If an exception occurs release the lock. The dispose failed for some reason.
                // While this isn't good, we definitely don't want to be caught in a deadlock.
                ServerLocker.Release();
            }

            var localServerCopy = piipanTestServer;

            _httpClientFactory.Setup(n => n.CreateClient(typeof(TImplementation).Name)).Returns(localServerCopy.HttpClient);

            if (services.FirstOrDefault(n => n.ServiceType == typeof(IHttpClientFactory)) == null)
            {
                services.AddTransient<IHttpClientFactory>(n => _httpClientFactory.Object);
            }

            var mockTokenProvider = new Mock<ITokenProvider<TImplementation>>();
            mockTokenProvider.Setup(n => n.RetrieveAsync()).ReturnsAsync("");

            services.AddTransient<ITokenProvider<TImplementation>>(n => mockTokenProvider.Object);
            services.AddTransient<IAuthorizedApiClient<TImplementation>, AuthorizedJsonApiClient<TImplementation>>();
            services.AddTransient<TService, TImplementation>();
        }
    }
}
