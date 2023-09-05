using Microsoft.Extensions.Logging;
using Piipan.Shared.Cryptography;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;

namespace Piipan.States.Core.Service
{
    public class StateInfoService : IStateInfoService
    {
        private readonly IStateInfoDao _stateInfoDao;
        private readonly ICryptographyClient _cryptographyClient;
        private readonly ILogger<StateInfoDao> _logger;
        public StateInfoService(IStateInfoDao stateInfoDao, ICryptographyClient cryptographyClient, ILogger<StateInfoDao> logger)
        {
            _stateInfoDao = stateInfoDao;
            _cryptographyClient = cryptographyClient;
            _logger = logger;
        }

        /// <summary>
        /// Gets state by state_abbreviation and decrypts contact information
        /// </summary>
        /// <param name="state_abbreviation"></param>
        /// <returns></returns>
        public async Task<IState> GetDecryptedState(string state_abbreviation)
        {
            var result = await _stateInfoDao.GetStateByAbbreviation(state_abbreviation);
            if (result == null)
            {
                return null;
            }

            DecryptContactInfo((StateInfoDbo)result);
            return result;
        }

        /// <summary>
        /// Gets states and decrypts contact information 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IState>> GetDecryptedStates()
        {
            var result = await _stateInfoDao.GetStates();

            foreach (var state in result)
            {
                DecryptContactInfo((StateInfoDbo)state);
            }
            return result;
        }

        private void DecryptContactInfo(StateInfoDbo state)
        {
            try
            {
                state.Email = _cryptographyClient.DecryptFromBase64String(state.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to decrypt Email for {state.State}.");
            }

            try
            {
                state.Phone = _cryptographyClient.DecryptFromBase64String(state.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to decrypt Phone for {state.State}.");
            }

            try
            {
                state.EmailCc = _cryptographyClient.DecryptFromBase64String(state.EmailCc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to decrypt EmailCC for {state.State}.");
            }
        }
    }
}
