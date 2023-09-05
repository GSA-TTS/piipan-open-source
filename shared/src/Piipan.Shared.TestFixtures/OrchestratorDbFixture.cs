using System;
using System.Threading;
using Dapper;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class OrchestratorDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly string CollabConnectionString;

        public OrchestratorDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("ParticipantsDatabaseConnectionString");
            CollabConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");

            Initialize(ConnectionString);
            Initialize(CollabConnectionString);
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        public void Initialize(string connectionString)
        {
            var retries = 10;
            var wait = 2000; // ms

            while (retries >= 0)
            {
                try
                {
                    using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
                    {
                        conn.ConnectionString = connectionString;
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
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Close();
            }

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();
                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string perstateSql = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);
            string matchesSql = System.IO.File.ReadAllText("match-record.sql", System.Text.Encoding.UTF8);
            string createStateInfo = System.IO.File.ReadAllText("state-info.sql", System.Text.Encoding.UTF8);
            string insertStateInfo = System.IO.File.ReadAllText("insert-state-info.sql", System.Text.Encoding.UTF8);

            // Participants DB
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Execute(perstateSql);
                conn.Execute("INSERT INTO uploads(created_at, publisher ,upload_identifier, status) VALUES(now() at time zone 'utc', current_user,'test-etag', 'COMPLETE')");

                conn.Close();
            }

            // Collaboration DB
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Execute(matchesSql);
                conn.Execute(createStateInfo);
                conn.Execute(insertStateInfo);
                conn.Close();
            }
        }
        public void InsertUpload()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("INSERT INTO uploads(created_at, publisher,upload_identifier, status) VALUES(now() at time zone 'utc', current_user ,'test-etag', 'COMPLETE')");
                conn.Close();
            }
        }
        public void ClearParticipants()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM participants");

                conn.Close();
            }
        }

        public void ClearMatchRecords()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM match_res_events");
                conn.Execute("DELETE FROM matches");

                conn.Close();
            }
        }

        public int CountMatchRecords()
        {
            int count;

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM matches");

                conn.Close();
            }

            return count;
        }
    }
}
