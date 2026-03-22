using SecureHelpdesk.Domain.Common;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Domain.Entities;

public class TicketAuditHistory : BaseEntity
{
    public Guid TicketId { get; set; }
    public string PerformedByUserId { get; set; } = string.Empty;
    public AuditActionType ActionType { get; set; }
    public string Description { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
    public ApplicationUser PerformedByUser { get; set; } = null!;
}
