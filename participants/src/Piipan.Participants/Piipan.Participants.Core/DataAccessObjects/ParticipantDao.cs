using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.NodaTime;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IParticipantBulkInsertHandler _bulkInsertHandler;
        private readonly ILogger<ParticipantDao> _logger;
        private readonly IDatabaseManager<ParticipantsDbManager> _databaseManager;

        public ParticipantDao(
            IParticipantBulkInsertHandler bulkInsertHandler,
            ILogger<ParticipantDao> logger,
            IDatabaseManager<ParticipantsDbManager> databaseManager)
        {
            _bulkInsertHandler = bulkInsertHandler;
            _logger = logger;
            this._databaseManager = databaseManager;
           
        }

        public async Task<IEnumerable<ParticipantDbo>> GetParticipants(string state, string ldsHash, Int64 uploadId)
        {
            return await _databaseManager.PerformQuery(async (connection) =>
            {
                return await connection
                    .QueryAsync<ParticipantDbo>(@"
                    SELECT
                        lds_hash LdsHash,
                        participant_id ParticipantId,
                        case_id CaseId,
                        participant_closing_date ParticipantClosingDate,
                        recent_benefit_issuance_dates RecentBenefitIssuanceDates,
                        vulnerable_individual VulnerableIndividual,
                        upload_id UploadId
                    FROM participants
                    WHERE lds_hash=@ldsHash
                        AND upload_id=@uploadId",
                        new
                        {
                            ldsHash = ldsHash,
                            uploadId = uploadId
                        }
                    );
            }, state);
        }

        public async Task<ulong> AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            return await _databaseManager.PerformQuery(async connection =>
            {
                try
                {
                    await connection.OpenAsync();
                    return await _bulkInsertHandler.LoadParticipants(participants, connection, "participants");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            });
        }
        public async Task DeleteOldParticipantsExcept(string state,  Int64 uploadId)
        {
            await _databaseManager.PerformQuery(async connection =>
            {
                var recordCount = await connection
                    .ExecuteAsync(@"
                    DELETE FROM participants
                    WHERE  upload_id<>@uploadId",
                        new
                        {
                            uploadId = uploadId
                        }
                    );

                if (String.IsNullOrEmpty(state))
                    _logger.LogInformation("Event Type : Outdated participant cleanup; Cleanup Time: {0} ; Records deleted :{1} ", DateTime.Now.ToString(), recordCount.ToString());
                else
                    _logger.LogInformation("Event Type : Outdated participant cleanup; Cleanup Time: {0} ; Records deleted :{1} ; State : {2}", DateTime.Now.ToString(), recordCount.ToString(), state);
                return recordCount;
            });
        }

    }
}
