using SecureHelpdesk.Application.DTOs.Auth;

namespace SecureHelpdesk.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<UserProfileDto> GetCurrentUserProfileAsync(string userId, CancellationToken cancellationToken);
}
