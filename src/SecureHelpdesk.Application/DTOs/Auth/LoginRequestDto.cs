using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Auth;

/// <summary>
/// Request payload for obtaining a JWT bearer token.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Registered email address for the account.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Plain-text password for the account.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; init; } = string.Empty;
}
