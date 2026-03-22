using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketListResponseDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required TicketPriority Priority { get; init; }
    public required TicketStatus Status { get; init; }
    public required string CreatedByUserId { get; init; }
    public required string CreatedByName { get; init; }
    public string? AssignedToUserId { get; init; }
    public string? AssignedToName { get; init; }
    public required int CommentCount { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
