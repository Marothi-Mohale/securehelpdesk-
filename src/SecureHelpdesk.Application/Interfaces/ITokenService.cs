using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITokenService
{
    Task<(string Token, DateTime ExpiresAtUtc)> CreateTokenAsync(ApplicationUser user, IReadOnlyCollection<string> roles);
}
