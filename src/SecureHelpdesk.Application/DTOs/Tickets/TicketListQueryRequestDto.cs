using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketListQueryRequestDto
{
    [StringLength(100, ErrorMessage = "Search must be 100 characters or fewer.")]
    public string? Search { get; init; }

    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Status must be a valid ticket status.")]
    public TicketStatus? Status { get; init; }

    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Priority must be a valid ticket priority.")]
    public TicketPriority? Priority { get; init; }

    public string? AssignedToUserId { get; init; }
    public string? CreatedByUserId { get; init; }

    [EnumDataType(typeof(TicketSortBy), ErrorMessage = "SortBy must be a valid sort field.")]
    public TicketSortBy SortBy { get; init; } = TicketSortBy.CreatedAt;

    [EnumDataType(typeof(SortDirection), ErrorMessage = "SortDirection must be a valid sort direction.")]
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 10;
}
