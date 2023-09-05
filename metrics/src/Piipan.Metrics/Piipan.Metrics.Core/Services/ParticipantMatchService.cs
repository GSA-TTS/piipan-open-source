using System.Linq;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.Services
{
    /// <summary>
    /// Service layer for creating, retrieving, updating Metrics MatchParticipant records
    /// </summary>
    public class ParticipantMatchService : IParticipantMatchWriterApi
    {
        private readonly IParticipantMatchDao _participantMatchDao;
      
        public ParticipantMatchService(IParticipantMatchDao participantMatchDao)
        {
            _participantMatchDao = participantMatchDao;
    
        }


        public async Task<int> AddMatchMetrics(ParticipantMatchMetrics participantMatch)
        {
            return await _participantMatchDao.AddParticipantMatchRecord(new ParticipantMatchDbo(participantMatch));
        }
        public async Task<int> UpdateMatchMetrics(ParticipantMatchMetrics participantMatch)
        {
            return await _participantMatchDao.UpdateParticipantMatchRecord(new ParticipantMatchDbo(participantMatch));
        }
        public async Task<int> PublishMatchMetrics(ParticipantMatchMetrics participantMatch)
        {
            // Check if the Match is already existing, if not create a new Match in the metrics
            var particpantMatchDbo = new ParticipantMatchDbo(participantMatch);
            var existingRecords = await _participantMatchDao.GetMatchMetrics(particpantMatchDbo.MatchId);
            if (existingRecords.Any())
            {
                return await _participantMatchDao.UpdateParticipantMatchRecord(new ParticipantMatchDbo(participantMatch));
            }
            else
            {
                return await _participantMatchDao.AddParticipantMatchRecord(particpantMatchDbo);
            }
        }
    }
}