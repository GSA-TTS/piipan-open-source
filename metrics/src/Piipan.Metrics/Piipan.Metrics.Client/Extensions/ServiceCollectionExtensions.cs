using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piipan.Metrics.Api;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;

namespace Piipan.Metrics.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMetricsClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            serviceCollection.Configure<AzureTokenProviderOptions<ParticipantUploadClient>>(options =>
            {
                var appId = Environment.GetEnvironmentVariable("MetricsApiAppId");
                options.ResourceUri = $"api://{appId}";
            });

            if (env.IsDevelopment())
            {
                serviceCollection.AddTransient<TokenCredential, AzureCliCredential>();
            }
            else
            {
                serviceCollection.AddTransient<TokenCredential, ManagedIdentityCredential>();
            }

            serviceCollection.AddHttpClient<ParticipantUploadClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("MetricsApiUri"));
            });

            serviceCollection.AddTransient<ITokenProvider<ParticipantUploadClient>, AzureTokenProvider<ParticipantUploadClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<ParticipantUploadClient>, AuthorizedJsonApiClient<ParticipantUploadClient>>();
            serviceCollection.AddTransient<IParticipantUploadReaderApi, ParticipantUploadClient>();
        }
    }
}
