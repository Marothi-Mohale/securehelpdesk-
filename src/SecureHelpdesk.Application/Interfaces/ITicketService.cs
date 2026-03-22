using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Models;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<PaginatedResponseDto<TicketListResponseDto>> GetTicketsAsync(TicketQueryParameters queryParameters, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketResponseDto> GetTicketByIdAsync(Guid ticketId, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketResponseDto> UpdateTicketAsync(Guid ticketId, UpdateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketResponseDto> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketResponseDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketResponseDto> AddCommentAsync(Guid ticketId, AddCommentRequestDto request, UserContext userContext, CancellationToken cancellationToken);
}
