using SecureHelpdesk.Domain.Common;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Domain.Entities;

public class TicketAuditLog : BaseEntity
{
    public Guid TicketId { get; set; }
    public AuditActionType ActionType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}
