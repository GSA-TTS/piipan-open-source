using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using System.Collections.Generic;

namespace Piipan.Match.Core.Builders
{
    public interface IMatchDetailsAggregator
    {
        MatchDetailsDto BuildAggregateMatchDetails(
            IMatchDbo match,
            IEnumerable<IMatchResEvent> match_res_events
        );
    }
}
