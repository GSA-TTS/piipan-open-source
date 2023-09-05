using System;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadWriterApi
    {
        Task<int> AddUploadMetrics(ParticipantUpload newParticipantUpload);
        Task<int> UpdateUploadMetrics(ParticipantUpload participantUpload); 

    }
}