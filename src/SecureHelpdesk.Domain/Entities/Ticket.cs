using SecureHelpdesk.Domain.Common;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ApplicationUser? AssignedToUser { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAuditLog> AuditLogs { get; set; } = new List<TicketAuditLog>();
}
