using System;
using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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
using Piipan.Shared.Tests.TestFixtures;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    /// <summary>
    /// Base class for Match Resolution API Integration tests. Implements IIntegrationTest to be used by Cypress integration tests
    /// and QT/Dashboard Integration tests.
    /// </summary>
    public class BaseMatchResIntegrationTests : DbFixture, IIntegrationTest
    {
        protected ServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Returns the Azure Function API of the given type
        /// </summary>
        /// <param name="type">The type of the Azure Function to find</param>
        /// <returns>An Azure Function object reference</returns>
        /// <exception cref="Exception">Exception thrown if Azure Function type is not registered</exception>
        public object GetApi(Type type)
        {
            if (type == typeof(AddEventApi))
            {
                return new AddEventApi(ServiceProvider.GetService<IMatchDao>(),
                    ServiceProvider.GetService<IMatchResEventDao>(),
                    ServiceProvider.GetService<IMatchDetailsAggregator>(),
                    ServiceProvider.GetService<IStreamParser<AddEventRequest>>(),
                    ServiceProvider.GetService<IParticipantPublishMatchMetric>());
            }
            if (type == typeof(GetMatchesApi))
            {
                return new GetMatchesApi(ServiceProvider.GetService<IMatchDao>(),
                    ServiceProvider.GetService<IMatchResEventDao>(),
                    ServiceProvider.GetService<IMatchDetailsAggregator>());
            }
            if (type == typeof(GetMatchApi))
            {
                return new GetMatchApi(ServiceProvider.GetService<IMatchDao>(),
                    ServiceProvider.GetService<IMatchResEventDao>(),
                    ServiceProvider.GetService<IMatchDetailsAggregator>(),
                    ServiceProvider.GetService<IStateInfoService>(),
                    ServiceProvider.GetService<IMemoryCache>());
            }
            throw new Exception($"API type of {type.Name} not registered");
        }

        /// <summary>
        /// Set up the environment variables and other services that the tests will use
        /// </summary>
        /// <returns></returns>
        public ServiceCollection SetupServices()
        {
            Environment.SetEnvironmentVariable("States", "ea");
            Environment.SetEnvironmentVariable("EventGridMetricMatchEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridMetricMatchKeyString", "example");
            Environment.SetEnvironmentVariable("EventGridNotifyEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridNotifyKeyString", "example");
            Environment.SetEnvironmentVariable("QueryToolUrl", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EnabledStates", "ea,eb");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IMatchDao, MatchDao>();
            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchDetailsAggregator, MatchDetailsAggregator>();
            services.AddTransient<IStateInfoDao, StateInfoDao>();
            services.AddTransient<IStateInfoService, StateInfoService>();
            services.AddTransient<IMemoryCache, MemoryCache>();
            services.AddTransient<IValidator<AddEventRequest>, AddEventRequestValidator>();
            services.AddTransient<IStreamParser<AddEventRequest>, AddEventRequestParser>();
            services.AddTransient<IParticipantPublishMatchMetric, ParticipantPublishMatchMetric>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<INotificationPublish, NotificationPublish>();
            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);
            services.AddTransient<IStateInfoService, StateInfoService>();

            services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
            {
                return new SingleDatabaseManager<CoreDbManager>(Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", key);
            services.RegisterKeyVaultClientServices();

            services.AddMvc()
                   .AddApplicationPart(typeof(Piipan.Notification.Common.ViewRenderService).GetTypeInfo().Assembly)
                   .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                   .AddDataAnnotationsLocalization();

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Templates/{0}" + RazorViewEngine.ViewExtension);
            });

            services.RegisterNotificationServices();

            ServiceProvider = services.BuildServiceProvider();

            return services;
        }


        protected T GetApi<T>() where T : class
        {
            return GetApi(typeof(T)) as T;
        }
    }
}
