using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class AuditLogResponseDto
{
    public required Guid Id { get; init; }
    public required AuditActionType ActionType { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public required string Description { get; init; }
    public required string ChangedByUserId { get; init; }
    public required string ChangedByName { get; init; }
    public required DateTime ChangedAtUtc { get; init; }
}
