using Newtonsoft.Json;
using Piipan.Participants.Api.Models;

namespace Piipan.Etl.Func.BulkUpload.Models
{
    /// <summary>
    /// Azure Function GetUploadStatus Response
    /// </summary>
    public class UploadStatusApiResponse
    {
        [JsonProperty("data", Required = Required.Always)]
        public UploadDto Data { get; set; }
    }
}
