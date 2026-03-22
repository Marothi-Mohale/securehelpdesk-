namespace SecureHelpdesk.Application.DTOs.Auth;

/// <summary>
/// Basic identity and authorization information for an authenticated user.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Unique user identifier from ASP.NET Core Identity.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name shown in helpdesk responses.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Primary sign-in email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role claims currently assigned to the user.
    /// </summary>
    public required IReadOnlyCollection<string> Roles { get; init; }
}
