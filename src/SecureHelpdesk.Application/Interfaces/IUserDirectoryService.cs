namespace SecureHelpdesk.Application.Interfaces;

public interface IUserDirectoryService
{
    Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> UserIsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken);
    Task<string> GetUserDisplayNameAsync(string userId, CancellationToken cancellationToken);
}
