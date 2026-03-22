using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Models;

public class TicketQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public string? AssigneeId { get; init; }
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
