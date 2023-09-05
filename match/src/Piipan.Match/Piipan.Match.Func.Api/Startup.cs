using System;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Extensions;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Validators;
using Piipan.Notification.Core.Extensions;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Parsers;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.Api.Startup))]

namespace Piipan.Match.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public const string ParticipantsDatabaseConnectionString = "ParticipantsDatabaseConnectionString";
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            builder.Services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            builder.Services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();
            builder.Services.AddTransient<IStateInfoService, StateInfoService>();

            builder.Services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            builder.Services.AddTransient<IMatchDetailsAggregator, MatchDetailsAggregator>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);

            builder.Services.AddSingleton<IDatabaseManager<ParticipantsDbManager>>((s) =>
                new MultipleDatabaseManager<ParticipantsDbManager>(Environment.GetEnvironmentVariable(ParticipantsDatabaseConnectionString),
                Environment.GetEnvironmentVariable("States")?.Split(',') ?? Array.Empty<string>()));

            builder.Services.AddSingleton<IDatabaseManager<CoreDbManager>>((s) =>
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
            builder.Services.RegisterMatchServices();
            builder.Services.RegisterParticipantsServices();
            builder.Services.RegisterKeyVaultClientServices();
        }
    }
}