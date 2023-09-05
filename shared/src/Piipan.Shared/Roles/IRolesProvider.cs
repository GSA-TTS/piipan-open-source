using System.Collections.Generic;

namespace Piipan.Shared.Roles
{
    /// <summary>
    /// The IRolesProvider will provide methods for the server to determine if a user has the roles to perform a function.
    /// The server can then pass these roles onto the Client so that the client can show/hide certain areas of the screen.
    /// </summary>
    public interface IRolesProvider
    {
        RoleOptions GetRoles();
        string[] TryGetRoles(string appArea);
    }
}
