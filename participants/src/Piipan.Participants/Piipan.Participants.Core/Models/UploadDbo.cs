using System;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.Models
{
    public record UploadDbo : IUpload
    {
        public Int64 Id { get; set; }
        public string UploadIdentifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Publisher { get; set; }
        public long? ParticipantsUploaded { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }
    }
}