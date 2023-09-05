using Piipan.Shared.Tests.DependencyInjection;
using Piipan.SupportTools.Core.Service;
using Piipan.SupportTools.Func.Api;
using Xunit;

namespace Piipan.SupportTools.Func.Api.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            var tester = new DependencyTester()
                .Register<PoisonMessageDequeuer>()
                .Register<PoisonMessageService>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}