using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantUploadDao
    {
        Task<Int64> GetUploadCount(ParticipantUploadRequestFilter filter);
        Task<IEnumerable<ParticipantUpload>> GetUploads(ParticipantUploadRequestFilter filter);
        Task<ParticipantUploadStatistics> GetUploadStatistics(ParticipantUploadStatisticsRequest filter);
        Task<IEnumerable<ParticipantUpload>> GetLatestSuccessfulUploadsByState();

        Task<int> AddUpload(ParticipantUploadDbo newUploadDbo);
        Task<int> UpdateUpload(ParticipantUploadDbo uploadDbo);
    }
}