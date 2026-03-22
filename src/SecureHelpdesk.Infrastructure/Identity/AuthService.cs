using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Mapping;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ApiException("A user with this email already exists.", StatusCodes.Status409Conflict);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ApiException(message, StatusCodes.Status400BadRequest);
        }

        await _userManager.AddToRoleAsync(user, RoleNames.User);
        _logger.LogInformation("Registered new user {UserId}", user.Id);

        return await CreateResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new ApiException("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new ApiException("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        _logger.LogInformation("User {UserId} logged in", user.Id);
        return await CreateResponseAsync(user);
    }

    private async Task<AuthResponseDto> CreateResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = await _tokenService.CreateTokenAsync(user, roles);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToUserProfileDto(roles)
        };
    }
}
