using Newtonsoft.Json;
using System.Collections.Generic;

namespace Piipan.Match.Api.Models.Resolution
{
    public class MatchResListApiResponse
    {
        [JsonProperty("data", Required = Required.Always)]
        public IEnumerable<MatchDetailsDto> Data { get; set; }
    }
}
