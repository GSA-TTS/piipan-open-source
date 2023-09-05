using System;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);
            Environment.SetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");

            Environment.SetEnvironmentVariable("EventGridNotifyEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridNotifyKeyString", "example");

            var tester = new DependencyTester()
                .Register<AddEventApi>()
                .Register<GetMatchApi>()
                .Register<GetMatchesApi>()
                .Register<MatchResEventNotify>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}