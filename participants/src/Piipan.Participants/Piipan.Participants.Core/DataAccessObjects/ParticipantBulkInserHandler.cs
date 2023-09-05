using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;
using Piipan.Participants.Core.Models;
using PostgreSQLCopyHelper;

namespace Piipan.Participants.Core.DataAccessObjects
{
    /// <summary>
    /// Helper for performing bulk insert operations of participant data
    /// into a PostgreSQL database.
    /// </summary>
    public class ParticipantBulkInsertHandler : IParticipantBulkInsertHandler
    {

        private readonly ILogger<ParticipantBulkInsertHandler> _logger;

        public ParticipantBulkInsertHandler(
            ILogger<ParticipantBulkInsertHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Bulk load a collection of participants into database
        /// </summary>
        /// <param name="participants">The collection of participant records to be loaded</param>
        /// <param name="dbConnection">The open database connection to be used</param>
        /// <param name="tableName">The name of the table where records will be loaded</param>
        /// <returns>Number of participant records loaded</returns>
        public async Task<ulong> LoadParticipants(IEnumerable<ParticipantDbo> participants, IDbConnection dbConnection, string tableName)
        {
            // PostgreSQL/Npgsql-specific operations require NpgsqlConnection
            NpgsqlConnection connection = dbConnection as NpgsqlConnection;

            var copyHelper = new PostgreSQLCopyHelper<ParticipantDbo>(tableName)
                .MapText("lds_hash", p => p.LdsHash)
                .MapText("participant_id", p => p.ParticipantId)
                .MapText("case_id", p => p.CaseId)
                .MapInteger("upload_id", p => (int)p.UploadId)
                .MapDate("participant_closing_date", p => p.ParticipantClosingDate)
                .Map("recent_benefit_issuance_dates", p =>
                    p.RecentBenefitIssuanceDates.Select(range =>
                        new DateInterval(
                            LocalDate.FromDateTime(range.Start),
                            LocalDate.FromDateTime(range.End)
                        )
                    ).ToArray<DateInterval>()
                )
                .MapBoolean("vulnerable_individual", p => p.VulnerableIndividual);

            _logger.LogInformation("Bulk inserting participant records");
            var result = await copyHelper.SaveAllAsync(connection, participants);
            _logger.LogInformation("Completed bulk insert of {0} participant records", result);

            return result;
        }
    }
}
