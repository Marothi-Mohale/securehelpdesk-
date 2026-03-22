using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketListQueryRequestDto
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; init; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; init; }

    public string? AssignedToUserId { get; init; }
    public string? CreatedByUserId { get; init; }

    [EnumDataType(typeof(TicketSortBy))]
    public TicketSortBy SortBy { get; init; } = TicketSortBy.CreatedAt;

    [EnumDataType(typeof(SortDirection))]
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
