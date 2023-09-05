using System;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Notification.Core.Extensions;
using Piipan.Notifications.Core.Services;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Parsers;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.ResolutionApi.Startup))]

namespace Piipan.Match.Func.ResolutionApi
{
    public class Startup : FunctionsStartup
    {
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IMatchDao, MatchDao>();
            builder.Services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            builder.Services.AddTransient<IParticipantPublishMatchMetric, ParticipantPublishMatchMetric>();
            builder.Services.AddTransient<IStateInfoDao, StateInfoDao>();
            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.AddTransient<INotificationPublish, NotificationPublish>();
            builder.Services.AddTransient<IMatchResNotifyService, MatchResNotifyService>();
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.AddTransient<IMatchDetailsAggregator, MatchDetailsAggregator>();
            builder.Services.AddTransient<IValidator<AddEventRequest>, AddEventRequestValidator>();
            builder.Services.AddTransient<IStreamParser<AddEventRequest>, AddEventRequestParser>();
            builder.Services.AddTransient<IStateInfoService, StateInfoService>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);

            builder.Services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
                new SingleDatabaseManager<CoreDbManager>(Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)));

           
            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            builder.Services.AddSingleton<DiagnosticListener>(listener);
            builder.Services.AddSingleton<DiagnosticSource>(listener);

            builder.Services
                    .AddMvcCore()
                    .AddViews()
                    .AddRazorViewEngine()
                    .AddApplicationPart(typeof(Piipan.Notification.Common.ViewRenderService).GetTypeInfo().Assembly)
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization();


            builder.Services.RegisterNotificationServices();
            builder.Services.RegisterKeyVaultClientServices();

        }
    }
}
