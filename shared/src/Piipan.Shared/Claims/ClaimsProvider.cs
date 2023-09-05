using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Claims
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsOptions _options;

        public ClaimsProvider(IOptions<ClaimsOptions> options)
        {
            _options = options.Value;
        }
       public string GetEmail(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal
                .Claims
                .SingleOrDefault(c => c.Type == _options.Email)?
                .Value;
        }

        public string GetLocation(ClaimsPrincipal claimsPrincipal)
        {
            foreach (var identity in claimsPrincipal.Identities)
            {
                var locationClaim = identity.Claims.FirstOrDefault(c => c.Type == _options.Role && c.Value.StartsWith(_options.LocationPrefix));
                if (locationClaim != null)
                {
                    return locationClaim.Value.Substring(_options.LocationPrefix.Length);
                }
            }
            return null;
        }
        public string GetRole(ClaimsPrincipal claimsPrincipal)
        {
            foreach (var identity in claimsPrincipal.Identities)
            {
                var roleClaim = identity.Claims.FirstOrDefault(c => c.Type == _options.Role && c.Value.StartsWith(_options.RolePrefix));
                if (roleClaim != null)
                {
                    return roleClaim.Value.Substring(_options.RolePrefix.Length);
                }
            }
            return null;
        }
    }
}