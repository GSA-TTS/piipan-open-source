using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Match.Core.DataAccessObjects
{
    /// <summary>
    /// Data Access Object for match records
    /// </summary>
    public class MatchDao : IMatchDao
    {
        private readonly IDatabaseManager<CoreDbManager> _databaseManager;
       

        /// <summary>
        /// Initializes a new instance of MatchDao
        /// </summary>
        public MatchDao(
            IDatabaseManager<CoreDbManager> databaseManager)
        {
            _databaseManager = databaseManager;
           
            // Removing this Dapper config may cause null values in expected columns
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>
        /// Adds a new match record to the database.
        /// </summary>
        /// <remarks>
        /// Throws `DuplicateMatchIdException` if a record with the incoming match ID already exists.
        /// </remarks>
        /// <param name="record">The MatchDbo object that will be added to the database.</param>
        /// <returns>Match ID for inserted record</returns>
        public async Task<string> AddRecord(MatchDbo record)
        {
            return await _databaseManager.PerformQuery(async (connection) =>
            {
                const string sql = @"
                INSERT INTO matches
                (
                    created_at,
                    match_id,
                    initiator,
                    states,
                    hash,
                    hash_type,
                    input,
                    data
                )
                VALUES
                (
                    now() at time zone 'utc',
                    @MatchId,
                    @Initiator,
                    @States,
                    @Hash,
                    @HashType::hash_type,
                    @Input::jsonb,
                    @Data::jsonb
                )
                RETURNING match_id;
            ";

                // Match IDs are randomly generated at the service level and may result
                // in unique constraint violations in the rare case of a collision.
                try
                {
                    return await connection.ExecuteScalarAsync<string>(sql, record);
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        throw new DuplicateMatchIdException(
                            "A record with the same Match ID already exists.",
                            ex);
                    }

                    throw ex;
                }
            });
        }

        public async Task<IEnumerable<IMatchDbo>> GetRecordsByHashAndState(MatchDbo record)
        {
            return await _databaseManager.PerformQuery(async (connection) =>
            {
                string sql = @"
                SELECT
                    match_id,
                    created_at,
                    initiator,
                    states,
                    hash,
                    hash_type::text,
                    input::jsonb,
                    data::jsonb
                FROM matches
                WHERE
                    hash=@Hash AND
                    hash_type::text=@HashType AND
                    states @> @States AND
                    states <@ @States;";

                return await connection.QueryAsync<MatchDbo>(sql, record);
            });
        }

        public async Task<IEnumerable<IMatchDbo>> GetMatches()
        {
            const string sql = @"
                SELECT
                    match_id,
                    created_at,
                    initiator,
                    states,
                    hash,
                    hash_type::text,
                    input::jsonb,
                    data::jsonb
                FROM matches;";

            return await _databaseManager.PerformQuery(async connection =>
                await connection.QueryAsync<MatchDbo>(sql));
        }

        public async Task<IEnumerable<IMatchDbo>> GetMatchesById(string[] matchIds)
        {
            const string sql = @"
                SELECT
                    match_id,
                    created_at,
                    initiator,
                    states,
                    hash,
                    hash_type::text,
                    input::jsonb,
                    data::jsonb
                FROM matches
                WHERE
                    match_id = ANY (@MatchIds)
                ;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.QueryAsync<MatchDbo>(sql,
                    new
                    {
                        MatchIds = matchIds?.Select(n => n.ToUpper()).ToArray()
                    });
            });
        }

        /// <summary>
        /// Finds a Match Record by Match ID
        /// </summary>
        /// <remarks>
        /// Throws InvalidOperationException if 0 or more than 1 record is found.
        /// </remarks>
        /// <param name="matchId">The Match ID for the specified match record.</param>
        /// <returns>Enumerable of Match Records with length 1</returns>
        public async Task<IMatchDbo> GetRecordByMatchId(string matchId)
        {
            const string sql = @"
                SELECT
                    match_id,
                    created_at,
                    initiator,
                    states,
                    hash,
                    hash_type::text,
                    input::jsonb,
                    data::jsonb
                FROM matches
                WHERE
                    match_id = @MatchId
                ;";

            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.QuerySingleAsync<MatchDbo>(sql, new MatchDbo
                {
                    MatchId = matchId?.ToUpper()
                });
            });
        }
    }
}
