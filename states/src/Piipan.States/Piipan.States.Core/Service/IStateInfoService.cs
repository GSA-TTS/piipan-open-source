using Piipan.States.Api.Models;

namespace Piipan.States.Core.Service
{
    public interface IStateInfoService
    {
        Task<IState> GetDecryptedState(string state_abbreviation);
        Task<IEnumerable<IState>> GetDecryptedStates();
    }
}
