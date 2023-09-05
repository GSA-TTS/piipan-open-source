using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Piipan.Match.Api;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;

namespace Piipan.Match.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMatchClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            serviceCollection.Configure<AzureTokenProviderOptions<MatchClient>>(options =>
            {
                var appId = Environment.GetEnvironmentVariable("OrchApiAppId");
                options.ResourceUri = $"api://{appId}";
            });

            // Add token credential services if it hasn't already been added
            if (env.IsDevelopment())
            {
                serviceCollection.TryAddTransient<TokenCredential, AzureCliCredential>();
            }
            else
            {
                serviceCollection.TryAddTransient<TokenCredential, ManagedIdentityCredential>();
            }

            serviceCollection.AddHttpClient<MatchClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OrchApiUri"));
            });
            serviceCollection.AddTransient<ITokenProvider<MatchClient>, AzureTokenProvider<MatchClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<MatchClient>, AuthorizedJsonApiClient<MatchClient>>();
            serviceCollection.AddTransient<IMatchSearchApi, MatchClient>();
        }

        public static void RegisterMatchResolutionClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            serviceCollection.Configure<AzureTokenProviderOptions<MatchResolutionClient>>(options =>
            {
                var appId = Environment.GetEnvironmentVariable("MatchResApiAppId");
                options.ResourceUri = $"api://{appId}";
            });

            // Add token credential services if it hasn't already been added
            if (env.IsDevelopment())
            {
                serviceCollection.TryAddTransient<TokenCredential, AzureCliCredential>();
            }
            else
            {
                serviceCollection.TryAddTransient<TokenCredential, ManagedIdentityCredential>();
            }

            serviceCollection.AddHttpClient<MatchResolutionClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("MatchResApiUri"));
            });
            serviceCollection.AddTransient<ITokenProvider<MatchResolutionClient>, AzureTokenProvider<MatchResolutionClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<MatchResolutionClient>, AuthorizedJsonApiClient<MatchResolutionClient>>();
            serviceCollection.AddTransient<IMatchResolutionApi, MatchResolutionClient>();
        }
    }
}
