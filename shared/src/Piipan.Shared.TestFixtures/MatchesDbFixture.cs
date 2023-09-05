using System;
using System.Threading;
using Dapper;
using Npgsql;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture for match records integration testing.
    /// Creates a fresh matches tables, dropping it when testing is complete.
    /// Requires environment variable CollaborationDatabaseConnectionString to be set.
    /// </summary>
    public class MatchesDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public MatchesDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

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

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Execute("DROP TYPE IF EXISTS hash_type");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("match-record.sql", System.Text.Encoding.UTF8);
            string createStateInfo = System.IO.File.ReadAllText("state-info.sql", System.Text.Encoding.UTF8);
            string insertStateInfo = System.IO.File.ReadAllText("insert-state-info.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Execute("DROP TYPE IF EXISTS hash_type");
                conn.Execute(sqltext);
                conn.Execute(createStateInfo);
                conn.Execute(insertStateInfo);

                conn.Close();
            }
        }

        public void ClearMatchRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM matches");

                conn.Close();
            }
        }

        public void ClearMatchResEvents()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM match_res_events");

                conn.Close();
            }
        }

        public void UpdateMatchResEventCreateDateTo1HourAgo(int id, int hours = 1)
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute($"UPDATE match_res_events SET inserted_at = (NOW() - INTERVAL '{hours} HOUR') AT TIME ZONE 'utc' WHERE id = {id}");

                conn.Close();
            }
        }
    }
}
