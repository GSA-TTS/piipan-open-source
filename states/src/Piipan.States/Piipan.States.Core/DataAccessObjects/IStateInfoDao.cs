using Piipan.States.Api.Models;

namespace Piipan.States.Core.DataAccessObjects
{
    public interface IStateInfoDao
    {
        Task<IEnumerable<IState>> GetStates();
        Task<int> UpsertState(StateInfoDto insertingState);
        Task<IState> GetStateByAbbreviation(string value);
    }
}
