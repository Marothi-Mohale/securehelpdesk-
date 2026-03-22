using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Full ticket detail response including comments and audit history.
/// </summary>
public class TicketResponseDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required TicketPriority Priority { get; init; }
    public required TicketStatus Status { get; init; }
    public required string CreatedByUserId { get; init; }
    public required string CreatedByName { get; init; }
    public string? AssignedToUserId { get; init; }
    public string? AssignedToName { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public required IReadOnlyCollection<CommentResponseDto> Comments { get; init; }
    public required IReadOnlyCollection<AuditLogResponseDto> AuditLogs { get; init; }
}
