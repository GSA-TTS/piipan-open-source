using System;
using System.Collections.Generic;
using Dapper;
using Dapper.NodaTime;
using NodaTime;
using Npgsql;
using Piipan.Match.Core.Models;
using Piipan.Participants.Core;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Common;
using Piipan.Shared.Database;
using Piipan.Shared.TestFixtures;

namespace Piipan.Match.Func.Api.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : OrchestratorDbFixture
    {
        public MatchDbo GetMatchRecord(string matchId)
        {
            MatchDbo record;

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                record = conn.QuerySingle<MatchDbo>(
                    @"SELECT
                        match_id,
                        initiator,
                        states,
                        hash,
                        hash_type::text,
                        input::jsonb,
                        data::jsonb
                    FROM matches
                    WHERE match_id=@matchId;",
                    new { matchId = matchId });

                conn.Close();
            }

            return record;
        }

        public IEnumerable<MatchDbo> GetMatches()
        {
            IEnumerable<MatchDbo> records;

            using (var conn = TestFixtureDataSourceHelper.CreateConnection(ConnectionString))
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                records = conn.Query<MatchDbo>(
                    @"SELECT
                        match_id,
                        initiator,
                        states,
                        hash,
                        hash_type::text,
                        input::jsonb,
                        data::jsonb
                    FROM matches");

                conn.Close();
            }

            return records;
        }

        public void Insert(ParticipantDbo record)
        {
            var builder = new NpgsqlDataSourceBuilder(ConnectionString);
            builder.UseNodaTime();
            NpgsqlDataSource ds = NodaTimeSetup.Setup(builder);

            using (var conn = ds.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(record);
                parameters.Add("UploadId", lastval);

                if (record.ParticipantClosingDate != null)
                {
                    parameters.Add("ParticipantClosingDate", LocalDate.FromDateTime((DateTime)record.ParticipantClosingDate));
                }

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_issuance_dates, vulnerable_individual)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitIssuanceDates::daterange[], @VulnerableIndividual)",
                    parameters);

                conn.Close();
            }
        }
    }
}
