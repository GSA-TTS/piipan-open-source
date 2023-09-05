using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.Services
{
    /// <summary>
    /// Service layer for creating, retrieving, updating Metrics UploadParticipant records
    /// </summary>
    public class ParticipantUploadService : IParticipantUploadReaderApi, IParticipantUploadWriterApi
    {
        private readonly IParticipantUploadDao _participantUploadDao;
        private readonly IMetaBuilder _metaBuilder;

        public ParticipantUploadService(IParticipantUploadDao participantUploadDao, IMetaBuilder metaBuilder)
        {
            _participantUploadDao = participantUploadDao;
            _metaBuilder = metaBuilder;
        }

        public async Task<GetParticipantUploadsResponse> GetLatestUploadsByState()
        {
            var uploads = await _participantUploadDao.GetLatestSuccessfulUploadsByState();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = _metaBuilder.Build()
            };
        }

        public async Task<int> AddUploadMetrics(ParticipantUpload newParticipantUpload)
        {
            return await _participantUploadDao.AddUpload(new ParticipantUploadDbo(newParticipantUpload));
        }

        public async Task<int> UpdateUploadMetrics(ParticipantUpload participantUpload)
        {
            return await _participantUploadDao.UpdateUpload(new ParticipantUploadDbo(participantUpload));
        }

        public async Task<GetParticipantUploadsResponse> GetUploads(ParticipantUploadRequestFilter filter)
        {
            var uploads = await _participantUploadDao.GetUploads(filter);
            var total = await _participantUploadDao.GetUploadCount(filter);

            var meta = _metaBuilder
                .SetFilter(filter)
                .SetTotal(total)
                .Build();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = meta
            };
        }

        public async Task<ParticipantUploadStatistics> GetUploadStatistics(ParticipantUploadStatisticsRequest request)
        {
            return await _participantUploadDao.GetUploadStatistics(request);
        }
    }
}