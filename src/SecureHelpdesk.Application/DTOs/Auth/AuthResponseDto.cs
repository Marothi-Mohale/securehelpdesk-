namespace SecureHelpdesk.Application.DTOs.Auth;

public class AuthResponseDto
{
    public required string Token { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required UserProfileDto User { get; init; }
}
