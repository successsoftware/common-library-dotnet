using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SSS.CommonLib.Entensions;

public static class ClaimExtensions
{
    public static string GetClaimValue(this IEnumerable<Claim> claims, string claimType)
    {
        var claim = claims.FirstOrDefault(p => p.Type == claimType);

        return claim is null ? string.Empty : claim.Value;
    }
}