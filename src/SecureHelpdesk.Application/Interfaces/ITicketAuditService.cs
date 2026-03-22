using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketAuditService
{
    TicketAuditLog RecordTicketCreated(Ticket ticket, string changedByUserId);
    TicketAuditLog RecordStatusChanged(Ticket ticket, string changedByUserId, TicketStatus oldStatus, TicketStatus newStatus);
    TicketAuditLog RecordAssignmentChanged(Ticket ticket, string changedByUserId, string? oldAssigneeLabel, string newAssigneeLabel);
    TicketAuditLog RecordPriorityChanged(Ticket ticket, string changedByUserId, TicketPriority oldPriority, TicketPriority newPriority);
    TicketAuditLog RecordTicketUpdated(Ticket ticket, string changedByUserId, IReadOnlyCollection<string> updates);
    TicketAuditLog RecordCommentAdded(Ticket ticket, string changedByUserId, string commentContent);
}
