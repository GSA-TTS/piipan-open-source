using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Extensions;
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Metrics.Func.Collect.Startup))]

namespace Piipan.Metrics.Func.Collect
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public const string MetricsDatabaseConnectionString  = "MetricsDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
            {
                return new SingleDatabaseManager<CoreDbManager>(Environment.GetEnvironmentVariable(MetricsDatabaseConnectionString));
            });
            
            builder.Services.RegisterCoreServices();
        }
    }
}