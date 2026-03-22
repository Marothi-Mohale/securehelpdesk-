using Microsoft.AspNetCore.Identity;

namespace SecureHelpdesk.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<TicketComment> AuthoredComments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAuditLog> ChangedAuditLogs { get; set; } = new List<TicketAuditLog>();
}
