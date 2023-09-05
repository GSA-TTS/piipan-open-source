using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;
using Piipan.States.Api;

namespace Piipan.States.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterStatesClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            serviceCollection.Configure<AzureTokenProviderOptions<StatesClient>>(options =>
            {
                var appId = Environment.GetEnvironmentVariable("StatesApiAppId");
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

            serviceCollection.AddHttpClient<StatesClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("StatesApiUri"));
            });
            serviceCollection.AddTransient<ITokenProvider<StatesClient>, AzureTokenProvider<StatesClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<StatesClient>, AuthorizedJsonApiClient<StatesClient>>();
            serviceCollection.AddTransient<IStatesApi, StatesClient>();
        }
    }
}
