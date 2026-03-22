using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Mapping;
using SecureHelpdesk.Domain.Constants;

namespace SecureHelpdesk.Api.Controllers;

/// <summary>
/// Manages ticket creation, ticket workflows, assignment, and discussion.
/// </summary>
[Tags("Tickets")]
[ApiController]
[Route("api/tickets")]
[Authorize]
[Produces("application/json")]
public class TicketsController : ApiControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Returns a paginated list of tickets visible to the current user.
    /// </summary>
    /// <remarks>
    /// Supports pagination, keyword search, status filtering, priority filtering, creator filtering, assignee filtering, and sorting.
    /// Regular users see only their own tickets, while agents and admins see a broader support view based on their role.
    /// </remarks>
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
    /// <remarks>
    /// This is the best endpoint for demonstrating the full helpdesk workflow because it includes the conversation timeline and audit trail.
    /// </remarks>
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
    /// <remarks>
    /// Newly created tickets start in the <c>Open</c> status and automatically record an audit entry.
    /// </remarks>
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
    /// <remarks>
    /// Only Admins and the currently assigned Agent can update ticket details. Changes are captured in the audit history.
    /// </remarks>
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
    /// <remarks>
    /// Valid transitions are enforced by the service layer to keep ticket workflows sensible and auditable.
    /// </remarks>
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
    /// <remarks>
    /// Only Admins can assign tickets. The assignee must be an existing user in the <c>Agent</c> role.
    /// </remarks>
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
    /// <remarks>
    /// Comments are returned as their own resource and also appear in the ticket detail timeline.
    /// </remarks>
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
