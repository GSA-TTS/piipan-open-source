using System;
using System.Collections.Generic;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.States.Api.Models;

namespace Piipan.Shared.TestFixtures
{
    public class StateInfoDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public StateInfoDto InvalidStateInfoDto = new StateInfoDto()
        {
            Id = "99",
            State = "test",
            StateAbbreviation = "TT",
            Email = "abc",
            Phone = "abc",
            Region = "TEST",
            EmailCc = "abc"
        };

        public List<StateInfoDto> StateInfoDtos => new List<StateInfoDto>()
        {
            new StateInfoDto()
            {
                Id = "99",
                State = "test1",
                StateAbbreviation = "TT",
                Email = "m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj",
                Phone = "sajNo6NgQGJJ14MtpBvrCg==",
                Region = "TEST",
                EmailCc = "+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"
            },
            new StateInfoDto()
            {
                Id = "100",
                State = "test2",
                StateAbbreviation = "TS",
                Email = "wOtH5Jlp93v0UZnASU6C/7Xwvw1k/ga2vnoOYG9n27z2PY9U5d937x49u56LHwj+",
                Phone = "ZS/ynQUulF/xfko1pNZoOA==",
                Region = "TWO",
                EmailCc = "+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"
            }
        };

        public StateInfoDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Initialize();
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        private void Initialize()
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

        /// <summary>
        /// Clear states.
        /// </summary>
        public void Dispose()
        {
            ClearStates();
        }

        private void ApplySchema()
        {
            string createHashTypeSql = System.IO.File.ReadAllText("create-hash_type.sql", System.Text.Encoding.UTF8);
            string sqltext = System.IO.File.ReadAllText("state-info.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Execute(createHashTypeSql);
                conn.Execute(sqltext);
                conn.Execute("INSERT INTO state_info(id, state, state_abbreviation, email, phone, region,email_cc) VALUES('0', 'zero', 'TT', 'test@email.example', '5551234', 'ZERO','test-cc@email.example')");

                conn.Close();
            }
        }

        public void ClearStates()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("TRUNCATE TABLE state_info");

                conn.Close();
            }
        }
    }
}
