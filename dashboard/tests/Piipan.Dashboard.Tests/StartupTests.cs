using Piipan.Dashboard.Controllers;
using Piipan.Dashboard.Pages;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange

            var tester = new DependencyTester()
                .Register<NotAuthorizedModel>()
                .Register<ServerErrorModel>()
                .Register<SignedOutModel>()
                .Register<UploadsController>();

            // Act/Assert
            tester.ValidateWebServices<Startup>(Program.CreateHostBuilder);
        }

    }
}
