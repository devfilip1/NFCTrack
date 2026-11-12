using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ShortLink.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Authenticated user has no valid 'sub' claim.");
    }
}
