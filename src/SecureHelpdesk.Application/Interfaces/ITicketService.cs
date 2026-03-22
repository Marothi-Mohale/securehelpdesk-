using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Tickets;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketService
{
    Task<TicketDetailDto> CreateTicketAsync(CreateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters queryParameters, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketDetailDto> GetTicketByIdAsync(Guid ticketId, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketDetailDto> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketDetailDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken);
    Task<TicketDetailDto> AddCommentAsync(Guid ticketId, AddCommentRequestDto request, UserContext userContext, CancellationToken cancellationToken);
}
