using System;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Client.Extensions;
using Xunit;

namespace Piipan.Match.Client.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        public ServiceCollectionExtensionsTests()
        {
            Environment.SetEnvironmentVariable("OrchApiUri", "https://tts.test");
            Environment.SetEnvironmentVariable("MatchResApiUri", "https://tts.test");
        }
        [Fact]
        public void RegisterMatchClientServices_DevelopmentServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            // Act
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchSearchApi>());
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
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchSearchApi>());
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
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchSearchApi>());
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
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            var clientFactory = provider.GetService<IHttpClientFactory>();
            var client = clientFactory.CreateClient("MatchClient");
            Assert.Equal("https://tts.test/", client.BaseAddress.ToString());
        }

        [Fact]
        public void RegisterMatchResolutionClientServices_DevelopmentServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            // Act
            services.RegisterMatchResolutionClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchResolutionApi>());
            Assert.IsType<AzureCliCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchResolutionClientServices_StagingServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Staging;

            // Act
            services.RegisterMatchResolutionClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchResolutionApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchResolutionClientServices_ProductionServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Production;

            // Act
            services.RegisterMatchResolutionClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchResolutionApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchResolutionClientServices_HttpClientBaseAddressSet()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            // Act
            services.RegisterMatchResolutionClientServices(env);
            var provider = services.BuildServiceProvider();

            var clientFactory = provider.GetService<IHttpClientFactory>();
            var client = clientFactory.CreateClient("MatchResolutionClient");
            Assert.Equal("https://tts.test/", client.BaseAddress.ToString());
        }

        [Fact]
        public void RegisterBothMatchClientServices_VerifyOnlyOneTokenCredential()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;

            // Act
            services.RegisterMatchClientServices(env);
            int serviceCount = services.Count;
            services.RegisterMatchResolutionClientServices(env);

            // Assert
            // Register Match Resolution Client Services will add 6 services
            // if the token credential is not added, otherwise 7.
            Assert.Equal(serviceCount + 6, services.Count);
        }
    }
}