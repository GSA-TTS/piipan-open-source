using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Extensions;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Metrics.Api;
using Piipan.Notification.Common;
using Piipan.Notification.Common.Models;
using Piipan.Notification.Core.Extensions;
using Piipan.Notifications.Core.Services;
using Piipan.Participants.Core.Extensions;
using Piipan.Participants.Core.Models;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Parsers;
using Piipan.Shared.Tests.TestFixtures;
using Piipan.States.Core.Service;

namespace Piipan.Match.Func.Api.IntegrationTests
{
    /// <summary>
    /// Base class for Match API Integration tests. Implements IIntegrationTest to be used by Cypress integration tests
    /// and QT/Dashboard Integration tests.
    /// </summary>
    public class BaseMatchIntegrationTests : DbFixture, IIntegrationTest
    {
        protected const string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        protected ServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Returns the Azure Function API of the given type
        /// </summary>
        /// <param name="type">The type of the Azure Function to find</param>
        /// <returns>An Azure Function object reference</returns>
        /// <exception cref="Exception">Exception thrown if Azure Function type is not registered</exception>
        public object GetApi(Type type)
        {
            if (type == typeof(MatchApi))
            {
                return new MatchApi(
                    ServiceProvider.GetService<IMatchSearchApi>(),
                    ServiceProvider.GetService<IStreamParser<OrchMatchRequest>>(),
                    ServiceProvider.GetService<IMatchEventService>(),
                    ServiceProvider.GetService<IMemoryCache>()
                );
            }
            throw new Exception($"API type of {type.Name} not registered");
        }

        /// <summary>
        /// Set up the environment variables and other services that the tests will use
        /// </summary>
        /// <returns>The service collection with environment variables and services registered</returns>
        public ServiceCollection SetupServices()
        {
            Environment.SetEnvironmentVariable("States", "ea");
            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");
            Environment.SetEnvironmentVariable("EventGridMetricSearchEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridMetricSearchKeyString", "example");
            Environment.SetEnvironmentVariable("EventGridMetricMatchEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridMetricMatchKeyString", "example");
            Environment.SetEnvironmentVariable("EventGridNotifyEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridNotifyKeyString", "example");
            Environment.SetEnvironmentVariable("QueryToolUrl", "http://someendpoint.gov");

            // Mixing cases to verify the enabled states can be used no matter their casing.
            Environment.SetEnvironmentVariable("EnabledStates", "ea,EB");

            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchDetailsAggregator, MatchDetailsAggregator>();
            services.AddTransient<IStateInfoService, StateInfoService>();
            services.AddSingleton<IMemoryCache, MemoryCache>();

            services.AddTransient<IParticipantPublishSearchMetric>(b =>
            {
                var factory = new Mock<IParticipantPublishSearchMetric>();

                factory.Setup(m => m.PublishSearchMetrics(
                                        It.IsAny<ParticipantSearchMetrics>()))
                       .Returns(Task.CompletedTask);

                return factory.Object;
            });

            services.AddSingleton<IDatabaseManager<ParticipantsDbManager>>(s =>
            {
                return new SingleDatabaseManager<ParticipantsDbManager>(Environment.GetEnvironmentVariable(Startup.ParticipantsDatabaseConnectionString));
            });

            services.AddTransient<IParticipantPublishMatchMetric>(b =>
            {
                var factory = new Mock<IParticipantPublishMatchMetric>();

                factory.Setup(m => m.PublishMatchMetric(
                                        It.IsAny<ParticipantMatchMetrics>()))
                       .Returns(Task.CompletedTask);

                return factory.Object;
            });



            services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
            {
                return new SingleDatabaseManager<CoreDbManager>(
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });
            services.AddTransient<IViewRenderService, ViewRenderService>();

            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);

            services.AddMvcCore()
                .AddViews()
                .AddRazorViewEngine()
                   .AddApplicationPart(typeof(Piipan.Notification.Common.ViewRenderService).GetTypeInfo().Assembly)
                   .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                   .AddDataAnnotationsLocalization();

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Templates/{0}" + RazorViewEngine.ViewExtension);
            });
            services.RegisterNotificationServices();
            services.RegisterParticipantsServices();
            services.RegisterMatchServices();
            services.RegisterKeyVaultClientServices();

            services.AddTransient<INotificationService>(s =>
            {
                var factory = new Mock<INotificationService>();

                factory.Setup(m => m.PublishNotificationOnMatchCreation(
                                        It.IsAny<NotificationRecord>()))
                       .ReturnsAsync(true);

                return factory.Object;
            });

            ServiceProvider = services.BuildServiceProvider();
            return services;
        }

        /// <summary>
        /// The default Farrington record. To be used by multiple tests.
        /// </summary>
        /// <returns>Farrington participant</returns>
        public ParticipantDbo GetDefaultRecord()
        {
            return new ParticipantDbo
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec",
                CaseId = "CaseIdExample",
                ParticipantId = "participantid1",
                ParticipantClosingDate = new DateTime(1970, 1, 15),
                RecentBenefitIssuanceDates = new List<DateRange>
                {
                    new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                    new DateRange(new DateTime(2021, 6, 1),new DateTime(2021, 7, 1)),
                    new DateRange(new DateTime(2021, 02, 28),new DateTime(2021, 3, 15))
                },
                VulnerableIndividual = true
            };
        }

        /// <summary>
        /// Encrypts a Participant record. To be used by multiple tests before inserting seed data
        /// </summary>
        public ParticipantDbo GetEncryptedFullRecord(ParticipantDbo unencryptedRecord)
        {
            AzureAesCryptographyClient client = new AzureAesCryptographyClient(base64EncodedKey);

            return new ParticipantDbo
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = client.EncryptToBase64String(unencryptedRecord.LdsHash),
                CaseId = client.EncryptToBase64String(unencryptedRecord.CaseId),
                ParticipantId = client.EncryptToBase64String(unencryptedRecord.ParticipantId),
                ParticipantClosingDate = unencryptedRecord.ParticipantClosingDate,
                RecentBenefitIssuanceDates = unencryptedRecord.RecentBenefitIssuanceDates,
                VulnerableIndividual = unencryptedRecord.VulnerableIndividual
            };
        }
    }
}
