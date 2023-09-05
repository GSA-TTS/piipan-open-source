using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Api
{
    public interface IMatchSearchApi
    {
        Task<OrchMatchResponse> FindAllMatches(OrchMatchRequest request, string initiatingState);
    }
}
