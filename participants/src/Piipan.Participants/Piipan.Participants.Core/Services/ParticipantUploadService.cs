using System;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;

namespace Piipan.Participants.Core.Services
{
    /// <summary>
    /// Service layer for managing participant upload metadata
    /// </summary>
    public class ParticipantUploadService : IParticipantUploadApi
    {
        private readonly IUploadDao _uploadDao;
      
        private readonly IParticipantPublishUploadMetric _participantPublishUploadMetric;

        public ParticipantUploadService(
            IUploadDao uploadDao, IParticipantPublishUploadMetric participantPublishUploadMetric)
        {
            _uploadDao = uploadDao;
            _participantPublishUploadMetric = participantPublishUploadMetric;
        }

        /// <summary>
        /// Adds metadata for a new upload.
        /// </summary>
        /// <param name="uploadIdentifier">The unique identifier of the upload to be added </param>
        /// <param name="state">The state generating the upload</param>
        /// <returns>The Upload record that was created in the database</returns>
        public async Task<UploadDto> AddUpload(string uploadIdentifier, string state)
        {
            var upload = await _uploadDao.AddUpload(uploadIdentifier);
            var result = new UploadDto(upload);

            //No Upload Metric is published here because a new Upload Metric event is automatically 
            //published to event grid with a BlobCreated event in Azure Storage i.e. when the upload
            //file is placed in Azure Storage

            return result;
        }

        /// <summary>
        /// Retrieves the metadata for the most recent successful upload
        /// </summary>
        /// <param name="state">The State we want to retrieve the latest upload for</param>
        /// <returns>The latest successful Upload</returns>
        public async Task<UploadDto> GetLatestUpload(string state = null)
        {
            var upload = await _uploadDao.GetLatestUpload(state);
            return new UploadDto(upload);
        }

        /// <summary>
        /// Retrieves upload metadata by id
        /// </summary>
        /// <param name="uploadIdentifier">The desired upload id for the upload we want to retrieve metadata</param>
        /// <returns>Upload whose upload identifier matches uploadIdentifier</returns>
        public async Task<UploadDto> GetUploadById(string uploadIdentifier)
        {
            var upload = await _uploadDao.GetUploadById(uploadIdentifier);
            return new UploadDto(upload);
        }

        /// <summary>
        /// Update metadata for an upload.
        /// </summary>
        /// <param name="upload">The upload record to add the error message to</param>
        /// <param name="state">The state who encountered the upload error</param>
        /// <param name="exceptionMessage">Optional- Details regarding the error encountered during the upload process</param>
        /// <returns></returns>
        public async Task<int> UpdateUpload(IUpload upload, string state, string exceptionMessage=null)
        {
            DateTime updateTime = DateTime.UtcNow;

            if(!string.IsNullOrEmpty(exceptionMessage))
            {
                upload.Status = UploadStatuses.FAILED.ToString();
                upload.ParticipantsUploaded = null;
            }

            upload.ErrorMessage = exceptionMessage;
            upload.CompletedAt = updateTime;
            var result = await _uploadDao.UpdateUpload(upload);

            var participantUploadMetrics = new ParticipantUpload();
            participantUploadMetrics.State = state;
            participantUploadMetrics.UploadIdentifier = upload.UploadIdentifier;
            participantUploadMetrics.Status = upload.Status;
            participantUploadMetrics.CompletedAt = upload.CompletedAt;
            participantUploadMetrics.ParticipantsUploaded = upload.ParticipantsUploaded;
            participantUploadMetrics.UploadedAt = upload.CreatedAt;
            participantUploadMetrics.ErrorMessage = exceptionMessage;

            await _participantPublishUploadMetric.PublishUploadMetric(participantUploadMetrics);

            return result;
        }
    }
}
