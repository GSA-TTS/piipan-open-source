using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.States.Func.Api.Tests
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

            var tester = new DependencyTester()
                .Register<StateApi>()
                .Register<UpsertState>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}
