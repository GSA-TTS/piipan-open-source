using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Match.Core.DataAccessObjects
{
    /// <summary>
    /// Data Access Object for match resolution events
    /// </summary>
    public class MatchResEventDao : IMatchResEventDao
    {
        private readonly IDatabaseManager<CoreDbManager> _databaseManager;

        /// <summary>
        /// Initializes a new instance of MatchResEventDao
        /// </summary>
        public MatchResEventDao(
            ILogger<MatchResEventDao> logger,
            IDatabaseManager<CoreDbManager> databaseManager)
        {
            // Removing this Dapper config may cause null values in expected columns
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Adds a new match resolution event to the database.
        /// </summary>
        /// <param name="record">The MatchResEventDbo object that will be added to the database.</param>
        /// <returns>ID for inserted record</returns>
        public async Task<int> AddEvent(MatchResEventDbo record)
        {
            const string sql = @"
                INSERT INTO match_res_events
                (
                    match_id,
                    actor,
                    actor_state,
                    delta,
                    notified_at
                )
                VALUES
                (
                    @MatchId,
                    @Actor,
                    @ActorState,
                    @Delta::jsonb,
                    null
                )
                RETURNING id
            ;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.ExecuteAsync(sql, record);
            });
        }

        /// <summary>
        /// Finds all match resolution events related to a match record
        /// </summary>
        /// <param name="matchId">The given match ID</param>
        /// <param name="sortByAsc">Boolean indicating ascending sort order, defaults to true. Argument of false is descending order</param>
        /// <returns>Task of IEnumerable of IMatchResEvents</returns>
        public async Task<IEnumerable<IMatchResEvent>> GetEventsByMatchId(
            string matchId,
            bool sortByAsc = true
        )
        {
            return await _databaseManager.PerformQuery(async (connection) =>
            {
                const string sql = @"
                SELECT
                    id,
                    match_id,
                    inserted_at,
                    actor,
                    actor_state,
                    delta::jsonb
                FROM match_res_events
                WHERE
                    match_id=@MatchId
                ORDER BY
                CASE
                    WHEN @SortByAsc = true THEN inserted_at
                END ASC,
                CASE
                    WHEN @SortByAsc = false THEN inserted_at
                END DESC
                ;";
                var parameters = new
                {
                    MatchId = matchId?.ToUpper(),
                    SortByAsc = sortByAsc
                };

                return await connection.QueryAsync<MatchResEventDbo>(sql, parameters);
            });
        }

        /// <summary>
        /// Finds all match resolution events related to a match record
        /// </summary>
        /// <param name="matchId">The given match ID</param>
        /// <param name="sortByAsc">Boolean indicating ascending sort order, defaults to true. Argument of false is descending order</param>
        /// <returns>Task of IEnumerable of IMatchResEvents</returns>
        public async Task<IEnumerable<IMatchResEvent>> GetEventsNotNotified()
        {
            // Bring back all of the matches where the last update was greater than 30 minutes ago.
            // If it has been updated again in the last 30 minutes, skip it.
            const string sql = @"
                SELECT
                    id,
                    match_id,
                    inserted_at,
                    actor,
                    actor_state,
                    delta::jsonb,
                    notified_at
                FROM match_res_events
                WHERE
                    match_id in 
                        (SELECT match_id from match_res_events
                            WHERE notified_at is null AND
                            EXTRACT(EPOCH FROM (NOW() AT TIME ZONE 'utc' - inserted_at)) / 60 > 30)
                ORDER BY inserted_at DESC
                ;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.QueryAsync<MatchResEventDbo>(sql);
            });
        }

        public async Task<int> UpdateMatchRecordsNotifiedAt(int[] ids)
        {
            const string sql = @"
            UPDATE match_res_events
            SET notified_at = NOW() AT TIME ZONE 'utc'
            WHERE
                id = ANY (@Ids)
            ;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.ExecuteAsync(sql,
                    new
                    {
                        Ids = ids
                    });
            });
        }

        /// <summary>
        /// Finds all match resolution events related to any of the specified match IDs
        /// </summary>
        /// <param name="matchIds">The list of match ID</param>
        /// <param name="sortByAsc">Boolean indicating ascending sort order, defaults to true. Argument of false is descending order</param>
        /// <returns>Task of IEnumerable of IMatchResEvents</returns>
        public async Task<IEnumerable<IMatchResEvent>> GetEventsByMatchIDs(
            IEnumerable<string> matchIds,
            bool sortByAsc = true
        )
        {
            const string sql = @"
                SELECT
                    id,
                    match_id,
                    inserted_at,
                    actor,
                    actor_state,
                    delta::jsonb,
                    notified_at
                FROM match_res_events
                WHERE
                    match_id = ANY (@MatchIds)
                ORDER BY
                CASE
                    WHEN @SortByAsc = true THEN inserted_at
                END ASC,
                CASE
                    WHEN @SortByAsc = false THEN inserted_at
                END DESC
                ;";
            var parameters = new
            {
                MatchIds = matchIds?.Select(n => n.ToUpper()).ToList(),
                SortByAsc = sortByAsc
            };

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.QueryAsync<MatchResEventDbo>(sql, parameters);
            });
        }
    }
}
