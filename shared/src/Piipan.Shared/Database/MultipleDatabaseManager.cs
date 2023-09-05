using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;

namespace Piipan.Shared.Database
{
    /// <summary>
    /// The MultipleDatabaseManager handles connections and queries to a set of databases.
    /// </summary>
    public class MultipleDatabaseManager<DbType> : BaseDatabaseManager<DbType>, IAsyncDisposable
    {

        Dictionary<string, (NpgsqlDataSource dataSource, string azureToken)> DataSources = new();
        Dictionary<string, SemaphoreSlim> ThreadLockers = new();

        private string _currentAzureToken = "";

        /// <summary>
        /// Create a new instance of ParticipantsDatabaseManager
        /// </summary>
        /// <param name="tokenProvider">An instance of AzureServiceTokenProvider</param>
        /// <param name="connectionString">The connection string used for building connections</param>
        /// 
        public MultipleDatabaseManager(
            string connectionString,
            string[] databaseNames,
            AzureServiceTokenProvider tokenProvider = null) : base(tokenProvider ?? new AzureServiceTokenProvider(), connectionString)
        {
            foreach (var database in databaseNames)
            {
                ThreadLockers.Add(database.ToLower(), new SemaphoreSlim(1));
                DataSources.Add(database.ToLower(), (null, null));
            }
        }

        /// <summary>
        /// Clean up semaphores and data sources
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            foreach (var locker in ThreadLockers)
            {
                await locker.Value.WaitAsync();
                try
                {
                    DataSources[locker.Key].dataSource?.Dispose();
                    DataSources[locker.Key] = (null, null);
                    locker.Value.Dispose();
                }
                catch
                {
                    // If an exception occurs release the lock. The dispose failed for some reason.
                    // While this isn't good, we definitely don't want to be caught in a deadlock.
                    locker.Value.Release();
                }
            }
        }

        /// <summary>
        /// Gets the connection to the specified database.
        /// </summary>
        /// <param name="database">The database to retrieve a connection string for</param>
        /// <param name="cancellationToken">Provides a way to cancel the transaction</param>
        /// <returns></returns>
        protected override async Task<NpgsqlConnection> GetConnectionAsync(string database, CancellationToken cancellationToken = default)
        {
            // If our password depends on the Azure token, grab the latest one. If it's different than our current one, update our password and create a new data source.
            if (ShouldFetchAzureToken)
            {
                var resourceId = CommercialId;
                var cn = Environment.GetEnvironmentVariable(CloudName);
                if (cn == GovernmentCloud)
                {
                    resourceId = GovermentId;
                }
                _currentAzureToken = await TokenProvider.GetAccessTokenAsync(resourceId);
            }
            if (!ThreadLockers.ContainsKey(database) || !DataSources.ContainsKey(database))
            {
                throw new ArgumentException("Database not registered in the data source dictionary.");
            }

            // Only one thread per database should be creating the datasource at a time so we don't have multiple database datasources floating around.
            await ThreadLockers[database].WaitAsync();
            try
            {
                var (dataSource, pastAzureToken) = DataSources[database];
                if (dataSource == null || pastAzureToken != _currentAzureToken)
                {
                    var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
                    builder.Database = database;

                    if (ShouldFetchAzureToken)
                    {
                        builder.Password = _currentAzureToken;
                    }
                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.ConnectionString);

                    // Dispose of the old dataSource for this database if one exists.
                    if (dataSource != null)
                    {
                        await dataSource.DisposeAsync();
                    }
                    // Use NodaTime as a convenience to handle converting DateInterval[]
                    // to a valid daterange[] input format
                    dataSourceBuilder.UseNodaTime();
                    DataSources[database] = (dataSourceBuilder.Build(), _currentAzureToken);
                }
                return DataSources[database].dataSource.CreateConnection();
            }
            finally
            {
                // Release our hold on the lock for this database so that other processes can go in here now.
                ThreadLockers[database].Release();
            }
        }
    }
}
