using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Services
{
    public interface IMatchEventService
    {
        Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState, string searchFrom, string[] enabledStatesList);
    }
}
