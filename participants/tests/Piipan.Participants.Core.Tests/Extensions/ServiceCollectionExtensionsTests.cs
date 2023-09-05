using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Participants.Core.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegisterCoreServices_AllResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(c => Mock.Of<IDbConnectionFactory<ParticipantsDb>>());
            services.AddSingleton<IDatabaseManager<ParticipantsDbManager>>(c => Mock.Of<IDatabaseManager<ParticipantsDbManager>>());

            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);


            // Act
            services.RegisterParticipantsServices();
            services.RegisterKeyVaultClientServices();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantDao>());
            Assert.NotNull(provider.GetService<IUploadDao>());
            Assert.NotNull(provider.GetService<IParticipantApi>());
            Assert.NotNull(provider.GetService<IParticipantBulkInsertHandler>());
        }
    }
}
