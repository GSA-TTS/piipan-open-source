using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.NodaTime;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;
using Polly.Retry;
using Polly;

namespace Piipan.Shared.Database
{
    /// <summary>
    /// The BaseDatabaseManager provides some common properties and constants for the other database managers to use.
    /// </summary>
    public abstract class BaseDatabaseManager<DbType> : IDatabaseManager<DbType>
    {
        // Environment variables (and placeholder) established
        // during initial function app provisioning in IaC
        public const string CloudName = "CloudName";
        protected const string PasswordPlaceholder = "{password}";
        public const string GovernmentCloud = "AzureUSGovernment";

        // Resource ids for open source software databases in the public and
        // US government clouds. Set the desired active cloud, then see:
        // `az cloud show --query endpoints.ossrdbmsResourceId`
        public const string CommercialId = "https://ossrdbms-aad.database.windows.net";
        public const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

        protected AzureServiceTokenProvider TokenProvider { get; }
        protected string ConnectionString { get; }
        protected bool ShouldFetchAzureToken { get; }

        public int NoOfRetries { get; set; } =  3;
        public int RetryInterval { get; set; } = 5;

        public BaseDatabaseManager(AzureServiceTokenProvider tokenProvider,
            string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string must be set to a value.");
            }
            TokenProvider = tokenProvider;
            ConnectionString = connectionString;
            ShouldFetchAzureToken = new NpgsqlConnectionStringBuilder(ConnectionString).Password == PasswordPlaceholder;

            SqlMapper.AddTypeHandler(new DateRangeListHandler());

            //*****************************************************
            //The following series of mappings sets up NodaTime type handlers. It feels a bit hacky.
            //With the introduction of npgsql v7, TypeMappers can no longer be added at the connection level.
            //They must be declared at the DataSource level. With Participants & Uploads, we have the need
            //to use transactions in order to commit to both tables at the same time. Trying to use two separate
            //datasources causes the transaction to escalate to a prepared transaction (or a distributed transaction)
            //which fails due to our Postgres server "max # of prepared transaction" settings. To avoid this, we're
            //using one datasource which means we need a mapper to go convert Nodatime to our Datetime properties
            //on our objects. DateTimeTypeHandler is a custom handler/mapper which can perform this conversion.
            SqlMapper.RemoveTypeMap(typeof(DateTime));

            DapperNodaTimeSetup.Register();

            SqlMapper.AddTypeHandler<DateTime>(new DateTimeTypeHandler());

            //*****************************************************
        }

        /// <summary>
        /// Gets the connection to the specified database.
        /// </summary>
        /// <param name="database">The database to retrieve a connection string for</param>
        /// <param name="cancellationToken">Provides a way to cancel the transaction</param>
        /// <returns></returns>
        protected abstract Task<NpgsqlConnection> GetConnectionAsync(string database, CancellationToken cancellationToken = default);


        /// <summary>
        /// Perform a query against the specified database.
        /// </summary>
        /// <typeparam name="T">The type to be returned by the function</typeparam>
        /// <param name="database">The database to perform a query against</param>
        /// <param name="query">A callback which executes the specified query with a connection string provided by the data source.</param>
        /// <param name="cancellationToken">Provides a way to cancel the transaction</param>
        /// <returns></returns>
        /// Retry policy error code details
        /// "08000" :	connection_exception
        /// "08003" :	connection_does_not_exist
        /// "08006" :	connection_failure
        /// "08001" :	sqlclient_unable_to_establish_sqlconnection
        /// "08004" :	sqlserver_rejected_establishment_of_sqlconnection
        /// "08007" :	transaction_resolution_unknown
        /// "53300" :	too_many_connections
        /// For more reference https://www.postgresql.org/docs/current/errcodes-appendix.html
        public virtual async Task<T> PerformQuery<T>(Func<NpgsqlConnection, Task<T>> query, string database = null, CancellationToken cancellationToken = default)
        {
            var retryableErrorCodes = new[] { "08000", "08001", "08007", "08006", "08004", "08003", "53300" };
            AsyncRetryPolicy retryPolicy = Policy
               .Handle<NpgsqlException>(ex => retryableErrorCodes.Any(errorCode => ex.SqlState.Contains(errorCode)))
               .Or<PostgresException>(ex => retryableErrorCodes.Any(errorCode => ex.SqlState.Contains(errorCode)))
                .WaitAndRetryAsync(NoOfRetries, _ => TimeSpan.FromSeconds(RetryInterval));

            return await retryPolicy.ExecuteAsync(async (ct) =>
            {
                await using var connection = await GetConnectionAsync(database?.ToLower(), cancellationToken);
                return await query(connection);

            }, CancellationToken.None);
        }
    }
}
