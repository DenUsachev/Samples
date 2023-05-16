using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Samshit.WebUtils
{
    public static class ClaimsHelper
    {
        public static IEnumerable<Claim> GetPrincipalClaimsByName(ClaimsPrincipal principal, string claimType)
        {
            if (principal != null)
            {
                return principal.Claims.Where(it => it.Type == claimType);
            }

            return Enumerable.Empty<Claim>();
        }
    }
}
