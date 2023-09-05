using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.States.Api.Models
{
    /// <summary>
    /// Represents the full API request from a client when using de-identified data
    /// </summary>
    public class StateInfoRequest
    {
        [JsonProperty("data", Required = Required.Always)]
        public StateInfoDto Data { get; set; } = new StateInfoDto();
    }
   
}
