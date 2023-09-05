using Dapper;
using Piipan.Shared.Database;
using Piipan.States.Api.Models;
using Piipan.States.Core.Models;

namespace Piipan.States.Core.DataAccessObjects
{
    /// <summary>
    /// This class retreives data from the state_info table in order to grab state email, phone, and region
    /// </summary>
    public class StateInfoDao : IStateInfoDao
    {
        private readonly IDatabaseManager<CoreDbManager> _databaseManager;

        public StateInfoDao(IDatabaseManager<CoreDbManager> databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Get single state info and searches off of abbreviation's parameter
        /// </summary>
        /// <param name="state_abbreviation"></param>
        /// <returns>Returns matching StateInfoDbo record</returns>
        public async Task<IState> GetStateByAbbreviation(string state_abbreviation)
        {
            try
            {
                StateInfoDbo result = await _databaseManager.PerformQuery(async connection =>
                {
                    return await connection
                        .QuerySingleAsync<StateInfoDbo>(@"
                    SELECT id, state, state_abbreviation as StateAbbreviation, email, phone, region, email_cc as EmailCc
	                FROM state_info WHERE state_abbreviation =@value
                    ORDER BY id DESC
                    LIMIT 1", new { value = state_abbreviation });
                });

                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all state info for all states
        /// </summary>
        /// <returns>Enumerable of StateInfoDbo Records</returns>
        public async Task<IEnumerable<IState>> GetStates()
        {
            return await _databaseManager.PerformQuery(async (connection) =>
            {
                var states = await connection
                    .QueryAsync<StateInfoDbo>(@"
                    SELECT id, state, state_abbreviation as StateAbbreviation, email, phone, region, email_cc as EmailCc
	                FROM state_info ORDER BY id ASC");

                return states;
            });
        }

        /// <summary>
        /// Inserts State record into state_info table
        /// </summary>
        /// <returns> StateInfoDbo Record</returns>
        public async Task<int> UpsertState(StateInfoDto insertingState)
        {
            const string sql = @"
                INSERT INTO public.state_info 
                    (id, state, state_abbreviation, email, phone, region, email_cc) 
                VALUES 
                    (@id, @state, @state_abbreviation, @email, @phone, @region, @email_cc) ON CONFLICT (state)
	            DO UPDATE SET 
                    id = @id, 
                    state = @state, 
                    state_abbreviation = @state_abbreviation, 
                    email = @email, 
                    phone = @phone, 
                    region = @region, 
                    email_cc = @email_cc";
            var parameters = new
            {
                id = insertingState.Id,
                state = insertingState.State,
                state_abbreviation = insertingState.StateAbbreviation,
                email = insertingState.Email,
                phone = insertingState.Phone,
                region = insertingState.Region,
                email_cc = insertingState.EmailCc,
            };

            return await _databaseManager.PerformQuery(async (connection) =>
            {
                return await connection.ExecuteAsync(sql, parameters);
            });
        }
    }
}
