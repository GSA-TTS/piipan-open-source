using System;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Metrics.Func.Collect.Tests
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
                .Register<CreateBulkUploadMetrics>()
                .Register<CreateSearchMetrics>()
                .Register<PublishMatchMetrics>()
                .Register<UpdateBulkUploadMetrics>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}
