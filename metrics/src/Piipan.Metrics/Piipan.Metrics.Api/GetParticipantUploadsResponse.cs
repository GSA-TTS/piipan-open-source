using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    public class GetParticipantUploadsResponse
    {
        [JsonProperty("data")]
        public IEnumerable<ParticipantUpload> Data;
        
        [JsonProperty("meta")]
        public Meta Meta;
    }
}