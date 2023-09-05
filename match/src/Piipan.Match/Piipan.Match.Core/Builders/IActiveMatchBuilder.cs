using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Core.Builders
{
    public interface IActiveMatchBuilder
    {
        IActiveMatchBuilder SetMatch(RequestPerson input, IParticipant match);
        IActiveMatchBuilder SetStates(string initiatingState, string matchingState);

        IMatchDbo GetRecord();
    }
}
