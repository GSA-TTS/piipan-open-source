using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Piipan.Shared.Authentication;

namespace Piipan.Shared.Cryptography.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterKeyVaultClientServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<TokenCredential, ManagedIdentityCredential>();
            var tokenCredential = new AzureCliCredential();
            serviceCollection.AddTransient<ITokenProvider<AzureRsaCryptographyClient>, AzureTokenProvider<AzureRsaCryptographyClient>>();
            serviceCollection.AddTransient<ICryptographyClient, AzureAesCryptographyClient>(options =>
            {
                var columnEncryptionKey = Environment.GetEnvironmentVariable("ColumnEncryptionKey");
                return new AzureAesCryptographyClient(columnEncryptionKey);
            });
        }
    }
}
