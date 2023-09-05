using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Piipan.Shared.Tests.Mocks;

namespace Piipan.Shared.Tests.TestFixtures
{
    /// <summary>
    /// This class acts as a test server for Integration Tests, allowing us to make calls to ASP.NET-exposed endpoints.
    /// We can now test the routing and make sure the correct endpoints are hit when calling an API URL.
    /// Example:
    ///   DuplicateParticipantQuery query = DefaultQuery();
    ///   using var piipanServer = new PiipanTestServer<Startup>(this, "EA");
    ///   var response = await piipanServer.HttpClient.PostAsJsonAsync("/api/duplicateparticipantsearch", query);
    /// </summary>
    /// <typeparam name="Startup"></typeparam>
    public class PiipanTestServer<Startup> : IDisposable where Startup : class
    {
        private TestServer _testServer;
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Creates the Piipan Test Server
        /// </summary>
        /// <param name="caller">The program calling this constructor. We pull the assembly name from this object, so make sure this object is an integration test object!</param>
        /// <param name="mockUserState">The user's mock state for authentication</param>
        /// <param name="configuration">In-memory settings for configuring the server</param>
        public PiipanTestServer(object caller, string mockUserState = "EA", IConfiguration configuration = null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            if (configuration == null)
            {
                var inMemorySettings = new Dictionary<string, string> {
                    {"Claims:Email", "email"},
                    {"Claims:Role", "role"},
                    {"Claims:LocationPrefix", "Location-"},
                    {"Claims:RolePrefix", "Role-"},
                    {"Locations:NationalOfficeValue", "National-"},
                    {"AuthorizationPolicy:RequiredClaims:0:Type", "email" },
                    {"AuthorizationPolicy:RequiredClaims:0:Values:0", "*"},
                };

                configuration = DefaultMocks.ConfigurationMock(inMemorySettings);
            }
            _testServer = new TestServer(
                new WebHostBuilder().UseSolutionRelativeContentRoot(TestDefaults.MockUserFile(caller.GetType().Assembly.GetName().Name, mockUserState))
                .UseStartup<Startup>((factory) =>
                {
                    factory.Configuration = configuration;
                    return typeof(Startup).GetConstructors()[0].Invoke(new object[] { configuration, factory.HostingEnvironment }) as Startup;
                }
                ));
            _testServer.BaseAddress = new Uri(_testServer.BaseAddress.ToString().Replace("http:", "https:"));
            HttpClient = _testServer.CreateClient();
            HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress.ToString().Replace("http:", "https:"));
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
