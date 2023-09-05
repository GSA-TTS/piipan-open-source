using System;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.Models
{
    /// <summary>
    /// Implementation of ParticipantUpload record for Metrics database interactions
    /// </summary>
    public class ParticipantUploadDbo
    {
        public string State { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; }
        public string UploadIdentifier { get; set; }
        public long? ParticipantsUploaded { get; set; }
        public string ErrorMessage { get; set; }

        public ParticipantUploadDbo()
        {
        }

        public ParticipantUploadDbo(ParticipantUpload upload)
        {
            State = upload.State;
            CompletedAt = upload.CompletedAt;
            UploadedAt = upload.UploadedAt;
            Status = upload.Status;
            UploadIdentifier = upload.UploadIdentifier;
            ParticipantsUploaded = upload.ParticipantsUploaded;
            ErrorMessage = upload.ErrorMessage;
        }
    }
}
