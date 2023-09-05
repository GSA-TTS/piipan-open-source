using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Api.Models.Resolution;

#nullable enable

namespace Piipan.Match.Api.Models
{
    /// <summary>
    /// Full request body for Add Event endpoint in Match Resolution API
    /// </summary>
    public class AddEventRequest
    {
        [JsonProperty("data",
            Required = Required.Always)]
        public Disposition Data { get; set; } = new Disposition();
    }
}
