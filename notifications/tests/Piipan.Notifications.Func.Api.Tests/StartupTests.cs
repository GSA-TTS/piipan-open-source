using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Notifications.Func.Api.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            Environment.SetEnvironmentVariable(Startup.SmtpConnection, "Test");
            Environment.SetEnvironmentVariable(Startup.EnableEmails, "true");
            Environment.SetEnvironmentVariable(Startup.SmtpCcEmail, "user1@something.test");
            Environment.SetEnvironmentVariable(Startup.SmtpBccEmail, "user2@something.test");
            Environment.SetEnvironmentVariable(Startup.SmtpFromEmail, "user3@something.test");

            var tester = new DependencyTester()
                .Register<NotificationApi>();

            // Act/Assert
            tester.ValidateFunctionServices<Startup>();
        }
    }
}
