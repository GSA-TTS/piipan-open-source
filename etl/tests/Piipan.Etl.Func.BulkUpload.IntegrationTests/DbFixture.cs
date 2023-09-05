using Dapper;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Participants.Api.Models;
using Piipan.Shared.TestFixtures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : IDisposable
    {
        public readonly string ConnectionString;

        public DbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("ParticipantsDatabaseConnectionString");

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
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "DROP TABLE IF EXISTS participants;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DROP TABLE IF EXISTS uploads;";
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        private void ApplySchema()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "DROP TABLE IF EXISTS participants;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DROP TABLE IF EXISTS uploads;";
                    cmd.ExecuteNonQuery();

                    string sqltext = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);
                    cmd.CommandText = sqltext;
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public void ClearParticipants()
        {
            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM participants";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public IEnumerable<IParticipant> QueryParticipants(string sql)
        {
            IEnumerable<IParticipant> records;

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.Open();
                records = conn.Query<Participant>(sql);
                conn.Close();
            }

            return records;
        }
    }
}
