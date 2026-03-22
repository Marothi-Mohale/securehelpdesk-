using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.Interfaces;

namespace SecureHelpdesk.Api.Controllers;

/// <summary>
/// Handles user registration, login, and the current authenticated user profile.
/// </summary>
[Tags("Auth")]
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new end user account and returns a bearer token for immediate use.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to create a normal user account for the helpdesk system. The response includes a JWT so the new user can call secured endpoints immediately.
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCurrentUser), null, response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token with role claims.
    /// </summary>
    /// <remarks>
    /// For local development, you can use the seeded demo accounts from the application configuration. Copy the returned token into Swagger's Authorize dialog to test secured endpoints.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Useful for verifying which identity and roles are currently attached to the bearer token in Swagger.
    /// </remarks>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var profile = await _authService.GetCurrentUserProfileAsync(CurrentUser.UserId, cancellationToken);
        return Ok(profile);
    }
}
