using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Piipan.SupportTools.Core.Service;

[assembly: FunctionsStartup(typeof(Piipan.SupportTools.Func.Api.Startup))]

namespace Piipan.SupportTools.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddTransient<IPoisonMessageService, PoisonMessageService>();
        }
    }
}
