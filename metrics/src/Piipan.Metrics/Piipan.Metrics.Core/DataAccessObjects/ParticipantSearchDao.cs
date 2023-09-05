using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Core.Models;
using Piipan.Shared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Core.DataAccessObjects
{
    /// <summary>
    /// Data access object for Participant Search records in Metrics database
    /// </summary>
    public class ParticipantSearchDao : IParticipantSearchDao
    {
        private readonly IDatabaseManager<CoreDbManager> _databaseManager;

        public ParticipantSearchDao(
            IDatabaseManager<CoreDbManager> databaseManager)
        {
            _databaseManager = databaseManager;
        }


        /// <summary>
        /// Adds a new Participant Upload record to the database.
        /// </summary>
        /// <param name="newSearchDbo">The ParticipantUploadDbo object that will be added to the database.</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> AddParticipantSearchRecord(ParticipantSearchDbo newSearchDbo)
        {
            return await _databaseManager.PerformQuery(async connection =>
            {
                return await connection.ExecuteAsync(@"
                    INSERT INTO participant_searches
                    (
                        state, 
                        search_reason,
                        search_from,
                        match_creation,
                        match_count,
                        searched_at
                   ) 
                    VALUES
                    (
                        @state, 
                        @search_reason,
                        @search_from,
                        @match_creation,
                        @match_count,
                        @searched_at
                    );",
                    new
                    {
                        state = newSearchDbo.State,
                        search_reason = newSearchDbo.SearchReason,
                        search_from = newSearchDbo.SearchFrom,
                        match_creation = newSearchDbo.MatchCreation,
                        match_count = newSearchDbo.MatchCount,
                        searched_at = newSearchDbo.SearchedAt
                    });
            });
        }
    }
}
