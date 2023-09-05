using System;
using Newtonsoft.Json;

namespace Piipan.Participants.Api.Models
{
    public class UploadDto : IUpload
    {
        public UploadDto()
        {

        }

        public UploadDto(IUpload upload)
        {
            Id = upload.Id;
            UploadIdentifier = upload.UploadIdentifier;
            CreatedAt = upload.CreatedAt;
            Publisher = upload.Publisher;
            ParticipantsUploaded = upload.ParticipantsUploaded;
            ErrorMessage = upload.ErrorMessage;
            CompletedAt = upload.CompletedAt;
            Status = upload.Status;
        }

        [JsonProperty("id")]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonProperty("upload_identifier")]
        public string UploadIdentifier { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("publisher")]
        [JsonIgnore]
        public string Publisher { get; set; }

        [JsonProperty("participants_uploaded",
            NullValueHandling = NullValueHandling.Ignore)]
        public long? ParticipantsUploaded { get; set; }

        [JsonProperty("error_message",
            NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [JsonProperty("completed_at",
            NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            UploadDto p = obj as UploadDto;
            if (p == null)
            {
                return false;
            }

            return
                Id == p.Id &&
                UploadIdentifier == p.UploadIdentifier &&
                CreatedAt == p.CreatedAt &&
                CompletedAt == p.CompletedAt &&
                Status == p.Status &&
                Publisher == p.Publisher &&
                ErrorMessage == p.ErrorMessage &&
                ParticipantsUploaded == p.ParticipantsUploaded;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                UploadIdentifier,
                CreatedAt,
                CompletedAt,
                Status,
                Publisher,
                ErrorMessage,
                ParticipantsUploaded
            );

        }
    }
}