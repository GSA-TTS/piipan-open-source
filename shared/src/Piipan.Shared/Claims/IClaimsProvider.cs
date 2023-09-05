using System.Security.Claims;

namespace Piipan.Shared.Claims
{
    public interface IClaimsProvider
    {
        string GetEmail(ClaimsPrincipal claimsPrincipal);
        string GetLocation(ClaimsPrincipal claimsPrincipal);
        string GetRole(ClaimsPrincipal claimsPrincipal);
    }
}