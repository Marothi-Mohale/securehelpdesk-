using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
