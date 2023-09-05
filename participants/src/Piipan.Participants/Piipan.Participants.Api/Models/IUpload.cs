using System;

namespace Piipan.Participants.Api.Models
{
    public interface IUpload
    {
        Int64 Id { get; set; }
        string UploadIdentifier { get; set; }
        DateTime CreatedAt { get; set; }
        string Publisher { get; set; }
        long? ParticipantsUploaded { get; set; }
        string ErrorMessage { get; set; }
        DateTime? CompletedAt { get; set; }
        string Status { get; set; }
    }
}