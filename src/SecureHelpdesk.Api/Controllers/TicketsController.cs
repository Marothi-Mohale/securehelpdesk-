using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureHelpdesk.Api.Extensions;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Mapping;
using SecureHelpdesk.Domain.Constants;

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
    [ProducesResponseType(typeof(PaginatedResponseDto<TicketListResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponseDto<TicketListResponseDto>>> GetTickets(
        [FromQuery] TicketListQueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetTicketsAsync(request.ToQueryParameters(), User.ToUserContext(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(ticketId, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent},{RoleNames.User}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket(CreateTicketRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateTicketAsync(request, User.ToUserContext(), cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { ticketId = ticket.Id }, ticket);
    }

    [HttpPatch("{ticketId:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicket(Guid ticketId, UpdateTicketRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateTicketAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPatch("{ticketId:guid}/status")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Agent}")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketResponseDto>> UpdateStatus(Guid ticketId, UpdateTicketStatusRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateStatusAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPatch("{ticketId:guid}/assign")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketResponseDto>> AssignTicket(Guid ticketId, AssignTicketRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.AssignTicketAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }

    [HttpPost("{ticketId:guid}/comments")]
    [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketResponseDto>> AddComment(Guid ticketId, AddCommentRequestDto request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.AddCommentAsync(ticketId, request, User.ToUserContext(), cancellationToken);
        return Ok(ticket);
    }
}
