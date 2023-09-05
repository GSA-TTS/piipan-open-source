using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Piipan.States.Api;
using Piipan.States.Client.Extensions;
using Xunit;

namespace Piipan.States.Client.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        public ServiceCollectionExtensionsTests()
        {
            Environment.SetEnvironmentVariable("StatesApiUri", "https://tts.test");
        }
        [Fact]
        public void RegisterStatesClientServices_DevelopmentServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            // Act
            services.RegisterStatesClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IStatesApi>());
            Assert.IsType<AzureCliCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_StagingServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Staging;

            // Act
            services.RegisterStatesClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IStatesApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_ProductionServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Production;

            // Act
            services.RegisterStatesClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IStatesApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_HttpClientBaseAddressSet()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            // Act
            services.RegisterStatesClientServices(env);
            var provider = services.BuildServiceProvider();

            var clientFactory = provider.GetService<IHttpClientFactory>();
            var client = clientFactory!.CreateClient("StatesClient");
            Assert.Equal("https://tts.test/", client.BaseAddress!.ToString());
        }
    }
}