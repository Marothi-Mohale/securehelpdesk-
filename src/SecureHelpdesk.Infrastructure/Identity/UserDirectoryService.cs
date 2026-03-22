using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Identity;

public class UserDirectoryService : IUserDirectoryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDirectoryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        return await _userManager.FindByIdAsync(userId) is not null;
    }

    public async Task<bool> UserIsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<string> GetUserDisplayNameAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new ApiException("User was not found.", StatusCodes.Status404NotFound);
        }

        return user.FullName;
    }
}
