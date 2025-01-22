using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Extensions;

public static class ClaimsPrincipalExtensions
{

    public static List<string>?Claim(this ClaimsPrincipal  claimsPrincipal, string claimType)
    {
        var result = claimsPrincipal?.FindAll(claimType)?.Select(x=>x.Value).ToList();
        return result;

    }

    public static List<string>? ClaimRoles(this ClaimsPrincipal claimsPrinsipal) => claimsPrinsipal?.Claim(ClaimTypes.Role);

    public static int GetUserId(this ClaimsPrincipal claimPrincipal) => Convert.ToInt32(claimPrincipal?.Claim(ClaimTypes.NameIdentifier)?.FirstOrDefault());

}
