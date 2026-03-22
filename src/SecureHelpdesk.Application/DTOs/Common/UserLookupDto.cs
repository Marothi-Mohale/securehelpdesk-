namespace SecureHelpdesk.Application.DTOs.Common;

/// <summary>
/// Lightweight user lookup item for assignment and selection UIs.
/// </summary>
public class UserLookupDto
{
    public required string Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
}
