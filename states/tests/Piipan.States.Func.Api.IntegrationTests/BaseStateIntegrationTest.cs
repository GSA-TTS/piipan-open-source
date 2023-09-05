using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Parsers;
using Piipan.Shared.TestFixtures;
using Piipan.Shared.Tests.TestFixtures;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Parsers;
using Piipan.States.Core.Service;

namespace Piipan.States.Func.Api.IntegrationTests
{
    /// <summary>
    /// Base class for State API Integration tests. Implements IIntegrationTest to be used by Cypress integration tests
    /// and QT/Dashboard Integration tests.
    /// </summary>
    public class BaseStateIntegrationTest : StateInfoDbFixture, IIntegrationTest
    {
        protected const string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        protected ServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Returns the Azure Function API of the given type
        /// </summary>
        /// <param name="type">The type of the Azure Function to find</param>
        /// <returns>An Azure Function object reference</returns>
        /// <exception cref="Exception">Exception thrown if Azure Function type is not registered</exception>
        public object GetApi(Type type)
        {
            ArgumentNullException.ThrowIfNull(ServiceProvider);
            if (type == typeof(StateApi))
            {
                return new StateApi(
                    ServiceProvider.GetService<IStateInfoService>()
                );
            }
            if (type == typeof(UpsertState))
            {
                return new UpsertState(
                    ServiceProvider.GetService<IStateInfoDao>(),
                    ServiceProvider.GetService<IStreamParser<StateInfoRequest>>(),
                    ServiceProvider.GetService<ICryptographyClient>()
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

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IStateInfoDao, StateInfoDao>();
            services.AddTransient<IStateInfoService, StateInfoService>();
            services.AddTransient<IStreamParser<StateInfoRequest>, StateInfoRequestParser>();

            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);
            var cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);

            services.AddSingleton<ICryptographyClient>(n => cryptographyClient);

            services.RegisterKeyVaultClientServices();
            services.AddSingleton<IDatabaseManager<CoreDbManager>>(s =>
            {
                return new SingleDatabaseManager<CoreDbManager>(
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            ServiceProvider = services.BuildServiceProvider();
            return services;
        }
    }
}
