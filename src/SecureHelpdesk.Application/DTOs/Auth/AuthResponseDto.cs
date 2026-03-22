namespace SecureHelpdesk.Application.DTOs.Auth;

/// <summary>
/// Authentication result containing a JWT access token and the authenticated user profile.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Signed JWT access token to send in the Authorization header.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Token type for Authorization header construction.
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// UTC timestamp indicating when the access token expires.
    /// </summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>
    /// Profile information for the authenticated user.
    /// </summary>
    public required UserProfileDto User { get; init; }
}
