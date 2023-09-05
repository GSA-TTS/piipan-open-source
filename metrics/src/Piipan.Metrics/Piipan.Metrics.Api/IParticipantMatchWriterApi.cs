using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public interface IParticipantMatchWriterApi
    {
        Task<int> AddMatchMetrics(ParticipantMatchMetrics participantMatch);
        Task<int> UpdateMatchMetrics(ParticipantMatchMetrics participantMatch);
        Task<int> PublishMatchMetrics(ParticipantMatchMetrics participantMatch);
    }
}