using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SecureHelpdesk.Application.Common;

namespace SecureHelpdesk.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static UserContext ToUserContext(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ApiException("Authenticated user context is missing a required identifier claim.", StatusCodes.Status401Unauthorized);

        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

        return new UserContext(userId, email, roles);
    }
}
