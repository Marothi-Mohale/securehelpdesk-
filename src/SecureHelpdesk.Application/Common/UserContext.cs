using SecureHelpdesk.Domain.Constants;

namespace SecureHelpdesk.Application.Common;

public sealed record UserContext(string UserId, string Email, IReadOnlyCollection<string> Roles)
{
    public bool IsAdmin => Roles.Contains(RoleNames.Admin, StringComparer.Ordinal);
    public bool IsAgent => Roles.Contains(RoleNames.Agent, StringComparer.Ordinal);
    public bool IsRegularUser => Roles.Contains(RoleNames.User, StringComparer.Ordinal);
}
