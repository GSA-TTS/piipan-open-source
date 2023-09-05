using System;
using System.Data.Common;
using System.Threading;
// using Microsoft.Extensions.Configuration;
using Dapper;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// Requires environment variable ParticipantsDatabaseConnectionString to be set.
    /// </summary>
    public class ParticipantsDbFixture : IDisposable
    {
        public readonly string ConnectionString;

        public ParticipantsDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("ParticipantsDatabaseConnectionString");

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
                    using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
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
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Execute(sqltext);
                conn.Execute("INSERT INTO uploads(created_at, publisher,upload_identifier, status) VALUES(now() at time zone 'utc', current_user,'test-etag', 'COMPLETE')");

                conn.Close();
            }
        }

        public void ClearParticipants(DbConnection conn)
        {
            conn.Execute("DELETE FROM participants");
        }

        public void ClearUploads(DbConnection conn)
        {
            conn.Execute("DELETE FROM uploads");
        }

        public Int64 GetLastUploadId(DbConnection conn)
        {
            var result = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
            return result;
        }

        public Int64 GetLastUploadIdWithStatus(string status, DbConnection conn)
        {
            var result = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads WHERE status=@uploadStatus", new { uploadStatus = status });
            return result;
        }
    }
}
