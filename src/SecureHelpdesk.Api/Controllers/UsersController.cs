using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Constants;

namespace SecureHelpdesk.Api.Controllers;

/// <summary>
/// Provides lightweight user lookups for demo and assignment scenarios.
/// </summary>
[Tags("Users")]
[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserDirectoryService _userDirectoryService;

    public UsersController(IUserDirectoryService userDirectoryService)
    {
        _userDirectoryService = userDirectoryService;
    }

    /// <summary>
    /// Returns agents available for ticket assignment.
    /// </summary>
    [HttpGet("agents")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserLookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserLookupDto>>> GetAgents(CancellationToken cancellationToken)
    {
        var agents = await _userDirectoryService.GetUsersInRoleAsync(RoleNames.Agent, cancellationToken);
        return Ok(agents);
    }
}
