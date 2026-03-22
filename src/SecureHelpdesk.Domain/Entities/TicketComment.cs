using SecureHelpdesk.Domain.Common;

namespace SecureHelpdesk.Domain.Entities;

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public ApplicationUser AuthorUser { get; set; } = null!;
}
