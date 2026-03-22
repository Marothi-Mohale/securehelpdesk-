using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required TicketStatus Status { get; init; }
    public required TicketPriority Priority { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
    public required string CreatedByUserId { get; init; }
    public required string CreatedByUserName { get; init; }
    public string? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public required IReadOnlyCollection<TicketCommentDto> Comments { get; init; }
    public required IReadOnlyCollection<TicketAuditHistoryDto> AuditHistory { get; init; }
}
