using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Query parameters for paginating, filtering, searching, and sorting ticket lists.
/// </summary>
public class TicketListQueryRequestDto
{
    /// <summary>
    /// Optional keyword search across ticket title and description.
    /// </summary>
    [StringLength(100, ErrorMessage = "Search must be 100 characters or fewer.")]
    public string? Search { get; init; }

    /// <summary>
    /// Optional workflow status filter.
    /// </summary>
    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Status must be a valid ticket status.")]
    public TicketStatus? Status { get; init; }

    /// <summary>
    /// Optional urgency filter.
    /// </summary>
    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Priority must be a valid ticket priority.")]
    public TicketPriority? Priority { get; init; }

    /// <summary>
    /// Optional filter for the currently assigned agent.
    /// </summary>
    public string? AssignedToUserId { get; init; }

    /// <summary>
    /// Optional filter for the ticket creator.
    /// </summary>
    public string? CreatedByUserId { get; init; }

    /// <summary>
    /// Sort field for the result set.
    /// </summary>
    [EnumDataType(typeof(TicketSortBy), ErrorMessage = "SortBy must be a valid sort field.")]
    public TicketSortBy SortBy { get; init; } = TicketSortBy.CreatedAt;

    /// <summary>
    /// Sort direction for the result set.
    /// </summary>
    [EnumDataType(typeof(SortDirection), ErrorMessage = "SortDirection must be a valid sort direction.")]
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    /// <summary>
    /// One-based page number.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items to return per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 10;
}
