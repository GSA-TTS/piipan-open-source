using Piipan.QueryTool.Controllers;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange

            var tester = new DependencyTester()
                .Register<ErrorModel>()
                .Register<NotAuthorizedModel>()
                .Register<ServerErrorModel>()
                .Register<SignedOutModel>()
                .Register<DuplicateParticipantSearchController>()
                .Register<MatchController>();

            // Act/Assert
            tester.ValidateWebServices<Startup>(Program.CreateHostBuilder);
        }

    }
}
