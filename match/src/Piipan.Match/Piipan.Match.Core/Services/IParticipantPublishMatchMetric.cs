using System.Threading.Tasks;
using Piipan.Metrics.Api;

namespace Piipan.Match.Core.Services
{
    public interface IParticipantPublishMatchMetric
    {
        Task PublishMatchMetric(ParticipantMatchMetrics metrics);
    }
}
