using System;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Metrics.Client.Extensions;
using Xunit;

namespace Piipan.Metrics.Client.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegisterMetricsClientServices_DevelopmentServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;
            Environment.SetEnvironmentVariable("MetricsApiUri", "https://tts.test");

            // Act
            services.RegisterMetricsClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantUploadReaderApi>());
            Assert.IsType<AzureCliCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMetricsClientServices_StagingServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Staging;
            Environment.SetEnvironmentVariable("MetricsApiUri", "https://tts.test");

            // Act
            services.RegisterMetricsClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantUploadReaderApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMetricsClientServices_ProductionServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Production;
            Environment.SetEnvironmentVariable("MetricsApiUri", "https://tts.test");

            // Act
            services.RegisterMetricsClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantUploadReaderApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMetricsClientServices_HttpClientBaseAddressSet()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;
            Environment.SetEnvironmentVariable("MetricsApiUri", "https://tts.test");

            // Act
            services.RegisterMetricsClientServices(env);
            var provider = services.BuildServiceProvider();

            var clientFactory = provider.GetService<IHttpClientFactory>();
            var client = clientFactory.CreateClient("ParticipantUploadClient");

            // Assert
            Assert.Equal("https://tts.test/", client.BaseAddress.ToString());
        }
    }
}