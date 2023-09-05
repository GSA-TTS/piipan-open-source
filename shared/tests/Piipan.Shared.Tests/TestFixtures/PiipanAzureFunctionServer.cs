using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Piipan.Shared.Tests.Mocks;

namespace Piipan.Shared.Tests.TestFixtures
{
    /// <summary>
    /// A server simulating an Azure Function. 
    /// All Azure Function APIs created with CreateWithPort are available to call.
    /// </summary>
    public class PiipanAzureFunctionServer : IDisposable
    {
        private TestServer _testServer;
        public IIntegrationTest IntegrationTest { get; set; }
        public HttpClient HttpClient { get; set; }

        private PiipanAzureFunctionServer(TestServer testServer)
        {
            _testServer = testServer;
            HttpClient = _testServer.CreateClient();
            HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress.ToString().Replace("http:", "https:"));
        }

        /// <summary>
        /// Creates a Azure Function mock server that runs at localhost:port
        /// All APIs passed in as params are available to call from the client.
        /// </summary>
        /// <typeparam name="TTest">The IIntegration test that we can call to setup the environment variables/services</typeparam>
        /// <param name="port">The port that this mock server runs on</param>
        /// <param name="apis">All of the APIs that need to be accounted for</param>
        /// <returns>The Azure Function mock server</returns>
        public static PiipanAzureFunctionServer CreateWithPort<TTest>(string port, params Type[] apis) where TTest : IIntegrationTest, new()
        {
            TTest test = new TTest();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            IConfiguration configuration = DefaultMocks.ConfigurationMock();
            AzureFunctionStartup startup = null;
            PiipanAzureFunctionServer piipanTestServer = null;
            var builder = new WebHostBuilder().UseStartup<AzureFunctionStartup>((factory) =>
            {
                factory.Configuration = configuration;
                var services = test.SetupServices();
                startup = new AzureFunctionStartup(services);
                foreach (Type type in apis)
                {
                    var api = test.GetApi(type);
                    startup.Register(type, api);
                }
                return startup;
            });

            var testServer = new TestServer(builder);
            testServer.CreateHandler();
            testServer.BaseAddress = new Uri((testServer.BaseAddress.ToString().Replace("http:", "https:")).Trim('/') + ":" + port);
            piipanTestServer = new PiipanAzureFunctionServer(testServer);
            piipanTestServer.IntegrationTest = test;
            return piipanTestServer;
        }

        /// <summary>
        /// Dispose of the server and its resources when it is closed.
        /// </summary>
        public void Dispose()
        {
            _testServer.Dispose();
            HttpClient.Dispose();
        }
    }
}
