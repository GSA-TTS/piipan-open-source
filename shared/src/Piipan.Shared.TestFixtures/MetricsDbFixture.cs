using System;
using System.Threading;
using Dapper;
using Npgsql;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture for metrics API database integration testing.
    /// Creates a fresh metrics database, dropping it when complete.
    /// Requires environment variable MetricsDatabaseConnectionString to be set in project.
    /// </summary>
    public class MetricsDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public MetricsDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("MetricsDatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Initialize();
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        public void Initialize()
        {
            var retries = 10;
            var wait = 2000; // ms

            while (retries >= 0)
            {
                try
                {
                    using (var conn = Factory.CreateConnection())
                    {
                        conn.ConnectionString = ConnectionString;
                        conn.Open();
                        conn.Close();

                        return;
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    retries--;
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(wait);
                }
            }
        }

        public void Dispose()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participant_uploads");
                conn.Execute("DROP TABLE IF EXISTS participant_searches");
                conn.Execute("DROP TABLE IF EXISTS participant_matches");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("metrics.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participant_uploads");
                conn.Execute("DROP TABLE IF EXISTS participant_searches");
                conn.Execute("DROP TABLE IF EXISTS participant_matches");
                conn.Execute(sqltext);

                conn.Close();
            }
        }
        public void ClearParticipantMatchesMetrics()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM participant_matches");

                conn.Close();
            }
        }
    }
}
