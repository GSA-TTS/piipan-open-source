using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    // <summary>
    /// Data Mapper for participant_searches table in metrics database
    /// </summary>
    public class ParticipantSearchMetrics
    {
        [JsonProperty("data", Required = Required.Always)]
        public List<ParticipantSearch> Data { get; set; } = new List<ParticipantSearch>();
    }
}
