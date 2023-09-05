using System;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// Data Mapper for participant_uploads table in metrics database
    /// </summary>
    public class ParticipantUpload
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("completed_at",
            NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("upload_identifier")]
        public string UploadIdentifier { get; set; }

        [JsonProperty("participants_uploaded",
            NullValueHandling = NullValueHandling.Ignore)]
        public long? ParticipantsUploaded { get; set; }

        [JsonProperty("error_message",
            NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

    }
}
