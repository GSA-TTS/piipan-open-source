using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Roles
{
    /// <summary>
    /// The RolesProvider will provide methods for the server to determine if a user has the roles to perform a function.
    /// The server can then pass these roles onto the Client so that the client can show/hide certain areas of the screen.
    /// </summary>
    public class RolesProvider : IRolesProvider
    {
        private readonly RoleOptions _options;

        public RolesProvider(IOptions<RoleOptions> options)
        {
            _options = options.Value;
        }

        public RoleOptions GetRoles()
        {
            return _options ?? new RoleOptions();
        }

        public string[] TryGetRoles(string appArea)
        {
            return _options?.ContainsKey(appArea) == true ? _options[appArea] : Array.Empty<string>();
        }
    }
}
