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
        var normalizedEmail = NormalizeEmail(request.Email);
        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw ApiException.Validation(
                "Full name is required.",
                new Dictionary<string, string[]>
                {
                    [nameof(request.FullName)] = ["Full name is required."]
                });
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["Email"] = LogSanitizer.MaskEmail(normalizedEmail)
        });

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw ApiException.Conflict("A user with this email already exists.", ErrorCodes.UserAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw CreateIdentityValidationException(result.Errors);
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!addToRoleResult.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                _logger.LogError(
                    "Failed to roll back user {UserId} after default role assignment failure",
                    user.Id);
            }

            throw CreateIdentityValidationException(addToRoleResult.Errors);
        }

        _logger.LogInformation("Registered new user {UserId} with default role {Role}", user.Id, RoleNames.User);

        return await CreateResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["Email"] = LogSanitizer.MaskEmail(normalizedEmail)
        });

        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for unknown account");
            throw ApiException.Unauthorized("Invalid email or password.", ErrorCodes.AuthInvalidCredentials);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Locked out user {UserId} attempted login", user.Id);
            throw ApiException.Unauthorized("Invalid email or password.", ErrorCodes.AuthInvalidCredentials);
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for user {UserId}", user.Id);
            throw ApiException.Unauthorized("Invalid email or password.", ErrorCodes.AuthInvalidCredentials);
        }

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return await CreateResponseAsync(user);
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw ApiException.NotFound("Authenticated user account was not found.", ErrorCodes.AuthUserNotFound);
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        return user.ToUserProfileDto(roles);
    }

    private async Task<AuthResponseDto> CreateResponseAsync(ApplicationUser user)
    {
        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var (token, expiresAtUtc) = await _tokenService.CreateTokenAsync(user, roles);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = user.ToUserProfileDto(roles)
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static ApiException CreateIdentityValidationException(IEnumerable<IdentityError> errors)
    {
        var materializedErrors = errors.ToArray();
        var validationErrors = materializedErrors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.Code) ? "identity" : error.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var detail = string.Join("; ", materializedErrors.Select(error => error.Description));
        return ApiException.Validation(detail, validationErrors);
    }
}
