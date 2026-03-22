using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Api.Extensions;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Mapping;
using SecureHelpdesk.Domain.Constants;

namespace SecureHelpdesk.Api.Controllers;

/// <summary>
/// Manages ticket creation, ticket workflows, assignment, and discussion.
/// </summary>
[ApiController]
[Route("api/tickets")]
[Authorize]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    private UserContext CurrentUser => User.ToUserContext();

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Returns a paginated list of tickets visible to the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponseDto<TicketListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResponseDto<TicketListResponseDto>>> GetTickets(
        [FromQuery] TicketListQueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetTicketsAsync(request.ToQueryParameters(), CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns ticket details, including comments and audit history, if the current user has access.
    /// </summary>
    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(ticketId, CurrentUser, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Creates a new helpdesk ticket for the current user.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent},{RoleNames.User}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket(
        [FromBody] CreateTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateTicketAsync(request, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { ticketId = ticket.Id }, ticket);
    }

    /// <summary>
    /// Updates editable ticket fields such as title, description, and priority.
    /// </summary>
    [HttpPatch("{ticketId:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicket(
        Guid ticketId,
        [FromBody] UpdateTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateTicketAsync(ticketId, request, CurrentUser, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Updates a ticket's workflow status when the requested transition is allowed.
    /// </summary>
    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicketStatus(
        Guid ticketId,
        [FromBody] UpdateTicketStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateStatusAsync(ticketId, request, CurrentUser, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Assigns a ticket to an agent.
    /// </summary>
    [HttpPatch("{ticketId:guid}/assignee")]
    [Authorize(Roles = RoleNames.Admin)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicketAssignee(
        Guid ticketId,
        [FromBody] AssignTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.AssignTicketAsync(ticketId, request, CurrentUser, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Adds a comment to a ticket visible to the current user.
    /// </summary>
    [HttpPost("{ticketId:guid}/comments")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponseDto>> AddComment(
        Guid ticketId,
        [FromBody] AddCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var comment = await _ticketService.AddCommentAsync(ticketId, request, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { ticketId }, comment);
    }
}
