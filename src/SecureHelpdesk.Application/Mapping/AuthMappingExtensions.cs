using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Mapping;

public static class AuthMappingExtensions
{
    public static UserProfileDto ToUserProfileDto(this ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}
