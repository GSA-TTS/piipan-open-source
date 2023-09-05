using System;
using System.Data.Common;
using System.Linq;
using Dapper;
using Piipan.Participants.Core.Models;
using Piipan.Shared.TestFixtures;

namespace Piipan.Participants.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : ParticipantsDbFixture
    {

        public void Insert(ParticipantDbo participant, DbConnection conn)
        {
            Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
            DynamicParameters parameters = new DynamicParameters(participant);
            parameters.Add("UploadId", lastval);

            conn.Execute(@"
                INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_issuance_dates, vulnerable_individual)
                VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitIssuanceDates::daterange[], @VulnerableIndividual)",
                parameters);
        }

        public void InsertUpload(string uploadId, DbConnection conn)
        {
            conn.Execute($"INSERT INTO uploads(created_at, publisher,upload_identifier, status) VALUES(now() at time zone 'utc', current_user ,'{uploadId}', 'COMPLETE')");
        }

        public bool HasParticipant(ParticipantDbo participant, DbConnection conn)
        {
            var result = false;
            
            var record = conn.Query<ParticipantDbo>(@"
                SELECT lds_hash LdsHash,
                    participant_id ParticipantId,
                    case_id CaseId,
                    participant_closing_date ParticipantClosingDate,
                    recent_benefit_issuance_dates RecentBenefitIssuanceDates,
                    vulnerable_individual VulnerableIndividual,
                    upload_id UploadId
                FROM participants
                WHERE lds_hash=@LdsHash", participant).FirstOrDefault();

            if (record == null)
            {
                result = false;
            }
            else
            {
                result = record.Equals(participant);
            }

            return result;
        }
    }
}
