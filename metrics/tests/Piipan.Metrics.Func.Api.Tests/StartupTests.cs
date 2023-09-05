using System;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            Environment.SetEnvironmentVariable(Startup.MetricsDatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            var tester = new DependencyTester()
                .Register<GetLastUpload>()
                .Register<GetParticipantUploads>()
                .Register<GetUploadStatistics>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}
