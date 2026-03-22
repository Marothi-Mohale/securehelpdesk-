using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Auth;

/// <summary>
/// Request payload for registering a new helpdesk end user.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// Full display name for the user account.
    /// </summary>
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Unique email address used for sign-in.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Plain-text password that will be hashed by ASP.NET Core Identity.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    public string Password { get; init; } = string.Empty;
}
