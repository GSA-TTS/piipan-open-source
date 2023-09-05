using System;
using Newtonsoft.Json;

#nullable enable

namespace Piipan.Metrics.Api
{
    public class Meta
    {
        [JsonProperty("perPage")]
        public int PerPage { get; set; }
        [JsonProperty("total")]
        public Int64 Total { get; set; }
        [JsonProperty("pageQueryParams")]
        public string? PageQueryParams { get; set; }
    }
}