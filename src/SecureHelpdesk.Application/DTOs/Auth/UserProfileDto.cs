namespace SecureHelpdesk.Application.DTOs.Auth;

public class UserProfileDto
{
    public required string Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
