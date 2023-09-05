using System.Threading.Tasks;
using Piipan.Metrics.Api;

namespace Piipan.Participants.Core.Services
{
    public interface IParticipantPublishUploadMetric
    {
        public Task PublishUploadMetric(ParticipantUpload metrics);
    }
}