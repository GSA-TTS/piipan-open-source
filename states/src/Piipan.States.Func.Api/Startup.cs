using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Parsers;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;
using Piipan.States.Core.Parsers;

[assembly: FunctionsStartup(typeof(Piipan.States.Func.Api.Startup))]

namespace Piipan.States.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IStateInfoDao, StateInfoDao>();
            builder.Services.AddTransient<IStreamParser<StateInfoRequest>, StateInfoRequestParser>();
            builder.Services.AddTransient<IStateInfoService, StateInfoService>();

            builder.Services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
                new SingleDatabaseManager<CoreDbManager>(Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)));

            

            builder.Services.RegisterKeyVaultClientServices();
        }
    }
}
