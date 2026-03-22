using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Api.Extensions;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] string? assigneeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new TicketQueryParameters
        {
            Status = status,
            Priority = priority,
            AssigneeId = assigneeId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _ticketService.GetTicketsAsync(query, User.ToUserContext(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(ticketId, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent},{RoleNames.User}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket(CreateTicketRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateTicketAsync(request, User.ToUserContext(), cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { ticketId = ticket.Id }, ticket);
    }

    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDetailDto>> UpdateStatus(Guid ticketId, UpdateTicketStatusRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateStatusAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPatch("{ticketId:guid}/assign")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDetailDto>> AssignTicket(Guid ticketId, AssignTicketRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.AssignTicketAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPost("{ticketId:guid}/comments")]
    [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketDetailDto>> AddComment(Guid ticketId, AddCommentRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.AddCommentAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }
}
