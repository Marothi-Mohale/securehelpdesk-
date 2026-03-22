using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketListQueryRequestDto
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; init; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; init; }

    public string? AssignedToUserId { get; init; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
