using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SecureHelpdesk.Application.Common;

namespace SecureHelpdesk.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static UserContext ToUserContext(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new BadHttpRequestException("Missing user identifier claim.");

        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

        return new UserContext(userId, email, roles);
    }
}
