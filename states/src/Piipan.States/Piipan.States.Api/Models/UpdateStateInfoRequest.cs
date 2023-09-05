using Piipan.States.Api.Models;
using System;

namespace Piipan.States.Api
{
    public record UpdateStateInfoRequest
    {
        public string id { get; set; }

        public StateInfoDto updatedStateInfo { get; set; }
    }
}
