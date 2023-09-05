using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantMatchDao
    {
        Task<int> AddParticipantMatchRecord(ParticipantMatchDbo newSearchDbo);
        Task<IEnumerable<ParticipantMatchMetrics>> GetMatchMetrics(string matchId);
        Task<IEnumerable<ParticipantMatchDbo>> GetRecords(ParticipantMatchDbo newSearchDbo);
        Task<int> UpdateParticipantMatchRecord(ParticipantMatchDbo matchDbo);
    }
}