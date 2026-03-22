using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketAuditHistoryDto
{
    public required Guid Id { get; init; }
    public required AuditActionType ActionType { get; init; }
    public required string Description { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string PerformedByUserId { get; init; }
    public required string PerformedByUserName { get; init; }
}
