using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Metrics.Core.DataAccessObjects
{
    /// <summary>
    /// Data access object for Participant Search records in Metrics database
    /// </summary>
    public class ParticipantMatchDao : IParticipantMatchDao
    {
        private readonly IDatabaseManager<CoreDbManager> _databaseManager;

        public ParticipantMatchDao(
            IDatabaseManager<CoreDbManager> databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Adds a new Participant Match record to the Metrics database.
        /// </summary>
        /// <param name="newSearchDbo">The ParticipantMatchDbo object that will be added to the database.</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> AddParticipantMatchRecord(ParticipantMatchDbo newSearchDbo)
        {
            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.ExecuteAsync(@"
                    INSERT INTO participant_matches(
	                    match_id,
                        match_created_at,
                        init_state,
                        init_state_invalid_match,
                        init_state_invalid_match_reason,
                        init_state_initial_action_taken,
                        init_state_initial_action_at,
                        init_state_final_disposition,
                        init_state_final_disposition_date,
                        init_state_vulnerable_individual,
                        matching_state,
                        matching_state_invalid_match,
                        matching_state_invalid_match_reason,
                        matching_state_initial_action_taken,
                        matching_state_initial_action_at,
                        matching_state_final_disposition,
                        matching_state_final_disposition_date,
                        matching_state_vulnerable_individual,
                        match_status
                    )
                    VALUES
                    (
                        @MatchId,
                        now() at time zone 'utc',
                        @InitState,
                        @InitStateInvalidMatch,
                        @InitStateInvalidMatchReason,
                        @InitStateInitialActionTaken,
                        @InitStateInitialActionAt,
                        @InitStateFinalDisposition,
                        @InitStateFinalDispositionDate,
                        @InitStateVulnerableIndividual,
                        @MatchingState,
                        @MatchingStateInvalidMatch,
                        @MatchingStateInvalidMatchReason,
                        @MatchingStateInitialActionTaken,
                        @MatchingStateInitialActionAt,
                        @MatchingStateFinalDisposition,
                        @MatchingStateFinalDispositionDate,
                        @MatchingStateVulnerableIndividual,
                        @Status
                    );",
                    new
                    {
                        MatchId = newSearchDbo.MatchId,
                        CreatedAt = newSearchDbo.CreatedAt,
                        InitState = newSearchDbo.InitState,
                        InitStateInvalidMatch = newSearchDbo.InitStateInvalidMatch,
                        InitStateInvalidMatchReason = newSearchDbo.InitStateInvalidMatchReason,
                        InitStateInitialActionAt = newSearchDbo.InitStateInitialActionAt,
                        InitStateInitialActionTaken = newSearchDbo.InitStateInitialActionTaken,
                        InitStateFinalDisposition = newSearchDbo.InitStateFinalDisposition,
                        InitStateFinalDispositionDate = newSearchDbo.InitStateFinalDispositionDate,
                        InitStateVulnerableIndividual = newSearchDbo.InitStateVulnerableIndividual,
                        MatchingState = newSearchDbo.MatchingState,
                        MatchingStateInvalidMatch = newSearchDbo.MatchingStateInvalidMatch,
                        MatchingStateInvalidMatchReason = newSearchDbo.MatchingStateInvalidMatchReason,
                        MatchingStateInitialActionAt = newSearchDbo.MatchingStateInitialActionAt,
                        MatchingStateInitialActionTaken = newSearchDbo.MatchingStateInitialActionTaken,
                        MatchingStateFinalDisposition = newSearchDbo.MatchingStateFinalDisposition,
                        MatchingStateFinalDispositionDate = newSearchDbo.MatchingStateFinalDispositionDate,
                        MatchingStateVulnerableIndividual = newSearchDbo.MatchingStateVulnerableIndividual,
                        Status = newSearchDbo.Status
                    });
            });
        }

        /// <summary>
        /// Update  Participant Match record in Metrics.
        /// </summary>
        /// <param name="matchDbo">The ParticipantMatchDbo object that will be added to the database.</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> UpdateParticipantMatchRecord(ParticipantMatchDbo matchDbo)
        {
            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.ExecuteAsync(@"
                    UPDATE participant_matches
                    SET  	
                        init_state= @InitState,
                        init_state_invalid_match= @InitStateInvalidMatch,
                        init_state_invalid_match_reason=@InitStateInvalidMatchReason,
                        init_state_initial_action_taken=@InitStateInitialActionTaken,
                        init_state_initial_action_at= @InitStateInitialActionAt,
                        init_state_final_disposition= @InitStateFinalDisposition,
                        init_state_final_disposition_date=@InitStateFinalDispositionDate,
                        init_state_vulnerable_individual= @InitStateVulnerableIndividual,
                        matching_state= @MatchingState,
                        matching_state_invalid_match= @MatchingStateInvalidMatch,
                        matching_state_invalid_match_reason=@MatchingStateInvalidMatchReason,
                        matching_state_initial_action_taken=@MatchingStateInitialActionTaken,
                        matching_state_initial_action_at= @MatchingStateInitialActionAt,
                        matching_state_final_disposition= @MatchingStateFinalDisposition,
                        matching_state_final_disposition_date=@MatchingStateFinalDispositionDate,
                        matching_state_vulnerable_individual= @MatchingStateVulnerableIndividual,
                        match_status= @Status
                    WHERE match_id= @MatchId;
                    ",
                    new
                    {
                        MatchId = matchDbo.MatchId,
                        CreatedAt = matchDbo.CreatedAt,
                        InitState = matchDbo.InitState,
                        InitStateInvalidMatch = matchDbo.InitStateInvalidMatch,
                        InitStateInvalidMatchReason = matchDbo.InitStateInvalidMatchReason,
                        InitStateInitialActionAt = matchDbo.InitStateInitialActionAt,
                        InitStateInitialActionTaken = matchDbo.InitStateInitialActionTaken,
                        InitStateFinalDisposition = matchDbo.InitStateFinalDisposition,
                        InitStateFinalDispositionDate = matchDbo.InitStateFinalDispositionDate,
                        InitStateVulnerableIndividual = matchDbo.InitStateVulnerableIndividual,
                        MatchingState = matchDbo.MatchingState,
                        MatchingStateInvalidMatch = matchDbo.MatchingStateInvalidMatch,
                        MatchingStateInvalidMatchReason = matchDbo.MatchingStateInvalidMatchReason,
                        MatchingStateInitialActionAt = matchDbo.MatchingStateInitialActionAt,
                        MatchingStateInitialActionTaken = matchDbo.MatchingStateInitialActionTaken,
                        MatchingStateFinalDisposition = matchDbo.MatchingStateFinalDisposition,
                        MatchingStateFinalDispositionDate = matchDbo.MatchingStateFinalDispositionDate,
                        MatchingStateVulnerableIndividual = matchDbo.MatchingStateVulnerableIndividual,
                        Status = matchDbo.Status
                    });
            });
        }

        /// <summary>
        /// Search for any exisiting match records in the Metrics database.
        /// </summary>
        /// <param name="newSearchDbo">The ParticipantMatchDbo object that will be searched in the database.</param>
        /// <returns>Number of rows affected</returns>
        ///
        public async Task<IEnumerable<ParticipantMatchDbo>> GetRecords(ParticipantMatchDbo newSearchDbo)
        {
            const string sql = @"
                 SELECT
	                    match_id
                FROM
                    participant_matches
                WHERE
                    match_id = @MatchId;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.QueryAsync<ParticipantMatchDbo>(sql, newSearchDbo);
            });
        }
        public async Task<IEnumerable<ParticipantMatchMetrics>> GetMatchMetrics(string matchId)
        {
            var sql = @"
                    SELECT
                        match_id MatchId,
                        match_created_at CreatedAt,
                        init_state InitState,
                        init_state_invalid_match InitStateInvalidMatch,
                        init_state_invalid_match_reason InitStateInvalidMatchReason,
                        init_state_initial_action_taken InitStateInitialActionTaken,
                        init_state_initial_action_at InitStateInitialActionAt,
                        init_state_final_disposition InitStateFinalDisposition,
                        init_state_final_disposition_date InitStateFinalDispositionDate,
                        init_state_vulnerable_individual InitStateVulnerableIndividual,
                        matching_state MatchingState,
                        matching_state_invalid_match MatchingStateInvalidMatch,
                        matching_state_invalid_match_reason MatchingStateInvalidMatchReason,
                        matching_state_initial_action_taken MatchingStateInitialActionTaken,
                        matching_state_initial_action_at MatchingStateInitialActionAt,
                        matching_state_final_disposition MatchingStateFinalDisposition,
                        matching_state_final_disposition_date matching_state_final_disposition_date,
                        matching_state_vulnerable_individual MatchingStateVulnerableIndividual,
                        match_status Status
                 FROM
                    participant_matches
                 WHERE
                    match_id = @MatchId;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection
                    .QueryAsync<ParticipantMatchMetrics>(sql, new { MatchId = matchId });
            });
        }
    }
}