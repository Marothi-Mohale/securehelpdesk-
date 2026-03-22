using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Models;

namespace SecureHelpdesk.Application.Mapping;

public static class TicketQueryMappingExtensions
{
    public static TicketQueryParameters ToQueryParameters(this TicketListQueryRequestDto request)
    {
        return new TicketQueryParameters
        {
            Status = request.Status,
            Priority = request.Priority,
            AssigneeId = request.AssignedToUserId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
