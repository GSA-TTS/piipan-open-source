using System.Threading.Tasks;
using Piipan.States.Api.Models;

namespace Piipan.Shared.Locations
{
    public interface ILocationsProvider
    {
        Task<string[]> GetStates(string location);

        Task<StatesInfoResponse> GetStatesFromStatesApi();
    }
}
