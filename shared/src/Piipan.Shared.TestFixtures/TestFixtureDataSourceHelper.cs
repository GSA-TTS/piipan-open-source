using Dapper;
using Dapper.NodaTime;
using Npgsql;
using Piipan.Participants.Core;
using Piipan.Shared.Common;
using Piipan.Shared.Database;
using System;
using System.Data.Common;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Meant to be used by Test DbFixture classes and Integration Tests when creating datasources and connections
    /// </summary>
    public class TestFixtureDataSourceHelper
    {
        /// <summary>
        /// Create a DbConnection for the provided connection string
        /// </summary>
        /// <param name="connectionString">Connection String of the database to establish a DbConnection</param>
        /// <returns>A DbConnection to the desired database</returns>
        public static DbConnection CreateConnection(string connectionString)
        {
            var ds = CreateDataSource(connectionString);
            var conn = ds.CreateConnection();
            return conn;
        }

        /// <summary>
        /// Create a DbConnection that uses NodaTime for the provided connection string
        /// </summary>
        /// <param name="connectionString">Connection String of the database to establish a DbConnection</param>
        /// <returns>A DbConnection to the desired database</returns>
        public static DbConnection CreateConnectionWithNodaTime(string connectionString)
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            builder.UseNodaTime();

            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler<DateTime>(new DateTimeTypeHandler());

            NpgsqlDataSource ds = NodaTimeSetup.Setup(builder);
            var conn = ds.CreateConnection();
            return conn;
        }

        /// <summary>
        /// Creates a DataSource for the provided connection string
        /// </summary>
        /// <param name="connectionString">Connection String of the database to establish a DataSource</param>
        /// <returns>A DataSource for the desired database</returns>
        public static NpgsqlDataSource CreateDataSource(string connectionString)
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            return builder.Build();
        }
    }
}
