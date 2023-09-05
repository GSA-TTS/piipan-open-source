using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;

namespace Piipan.Shared.Database
{
    /// <summary>
    /// The SingleDatabaseManager handles connections and queries to be performed on a single database.
    /// </summary>
    public class SingleDatabaseManager<T> : BaseDatabaseManager<T>, IAsyncDisposable
    {
        private NpgsqlDataSource _dataSource = null;
        private SemaphoreSlim _threadLocker = new SemaphoreSlim(1);
        private string _previousAzureToken = "";

        /// <summary>
        /// Create a new instance of SingleDatabaseManager
        /// </summary>
        /// <param name="tokenProvider">An instance of AzureServiceTokenProvider</param>
        /// <param name="connectionString">The connection string used for building connections</param>
        /// 
        public SingleDatabaseManager(
            string connectionString,
            AzureServiceTokenProvider tokenProvider = null) : base(tokenProvider ?? new AzureServiceTokenProvider(), connectionString)
        {
        }

        /// <summary>
        /// Clean up semaphores and data sources
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await _threadLocker.WaitAsync();
            try
            {
                _dataSource?.Dispose();
                _dataSource = null;
                _threadLocker.Dispose();
            }
            catch
            {
                // If an exception occurs release the lock. The dispose failed for some reason.
                // While this isn't good, we definitely don't want to be caught in a deadlock.
                _threadLocker.Release();
            }
        }

        /// <summary>
        /// Gets the connection to the database.
        /// </summary>
        /// <param name="database">This database is ignored for the SingleDatabaseManager as it is provided in the connection string</param>
        /// <param name="cancellationToken">Provides a way to cancel the transaction</param>
        /// <returns></returns>
        protected override async Task<NpgsqlConnection> GetConnectionAsync(string database, CancellationToken cancellationToken = default)
        {
            // Only one thread should be creating the datasource at a time so we don't have multiple datasources floating around.
            await _threadLocker.WaitAsync(cancellationToken);
            try
            {
                bool shouldCreateDataSource = _dataSource == null;
                string newAzureToken = "";

                // If our password depends on the Azure token, grab the latest one. If it's different than our current one, update our password and create a new data source.
                if (ShouldFetchAzureToken)
                {
                    var resourceId = CommercialId;
                    var cn = Environment.GetEnvironmentVariable(CloudName);
                    if (cn == GovernmentCloud)
                    {
                        resourceId = GovermentId;
                    }
                    newAzureToken = await TokenProvider.GetAccessTokenAsync(resourceId);
                    if (newAzureToken != _previousAzureToken)
                    {
                        shouldCreateDataSource = true;
                    }
                    _previousAzureToken = newAzureToken;
                }
                if (shouldCreateDataSource)
                {
                    var builder = new NpgsqlConnectionStringBuilder(ConnectionString);

                    if (ShouldFetchAzureToken)
                    {
                        builder.Password = newAzureToken;
                    }

                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.ConnectionString);

                    // If the data source currently exists we need to dispose of the old one.
                    if (_dataSource != null)
                    {
                        await _dataSource.DisposeAsync();
                    }
                    // Use NodaTime as a convenience to handle converting DateInterval[]
                    // to a valid daterange[] input format
                    dataSourceBuilder.UseNodaTime();
                    _dataSource = dataSourceBuilder.Build();
                }
            }
            finally
            {
                // Release our hold on the lock so that other processes can go in here now.
                _threadLocker.Release();
            }

            return _dataSource.CreateConnection();
        }
    }
}
