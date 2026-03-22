using SecureHelpdesk.Domain.Common;

namespace SecureHelpdesk.Domain.Entities;

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
