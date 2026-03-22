using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(3)]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int ExpirationMinutes { get; init; } = 60;
}
