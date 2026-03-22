using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Models;

namespace SecureHelpdesk.Application.Mapping;

public static class TicketQueryMappingExtensions
{
    public static TicketQueryParameters ToQueryParameters(this TicketListQueryRequestDto request)
    {
        return new TicketQueryParameters
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            Status = request.Status,
            Priority = request.Priority,
            AssigneeId = request.AssignedToUserId,
            CreatedByUserId = request.CreatedByUserId,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
