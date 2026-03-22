using SecureHelpdesk.Domain.Constants;

namespace SecureHelpdesk.Application.Common;

public sealed record UserContext(string UserId, string Email, IReadOnlyCollection<string> Roles)
{
    private HashSet<string> RoleSet { get; } = new(Roles, StringComparer.Ordinal);

    public bool IsAdmin => HasRole(RoleNames.Admin);
    public bool IsAgent => HasRole(RoleNames.Agent);
    public bool IsRegularUser => HasRole(RoleNames.User);

    public bool HasRole(string roleName)
    {
        return RoleSet.Contains(roleName);
    }
}
