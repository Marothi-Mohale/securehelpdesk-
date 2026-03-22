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

    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ApplicationUser? AssignedToUser { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAuditHistory> AuditHistory { get; set; } = new List<TicketAuditHistory>();
}
