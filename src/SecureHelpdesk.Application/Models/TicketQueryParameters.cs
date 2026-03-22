using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Models;

public class TicketQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public string? Search { get; init; }
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public string? AssignedToUserId { get; init; }
    public string? CreatedByUserId { get; init; }
    public TicketSortBy SortBy { get; init; } = TicketSortBy.CreatedAt;
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0 ? 10 : Math.Min(value, MaxPageSize);
    }
}
