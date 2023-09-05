using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Piipan.QueryTool.Client.Api;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Client.DTO;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var apiService = builder.Services.AddHttpClient<IQueryToolApiService, QueryToolApiService>("PiipanApi", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
        builder.Services.AddModalManager();
        builder.Services.AddPiipanNavigationManager();

        builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PiipanApi"));
        builder.Services.AddScoped<PiipanHttpMessageHandler>();
        apiService.AddHttpMessageHandler<PiipanHttpMessageHandler>();

        builder.Services.AddSingleton<ClientAppDataDto>();
        await builder.Build().RunAsync();
    }
}
