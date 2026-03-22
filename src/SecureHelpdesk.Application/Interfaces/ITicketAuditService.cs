using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketAuditService
{
    void RecordTicketCreated(Ticket ticket, string changedByUserId);
    void RecordStatusChanged(Ticket ticket, string changedByUserId, TicketStatus oldStatus, TicketStatus newStatus);
    void RecordAssignmentChanged(Ticket ticket, string changedByUserId, string? oldAssigneeLabel, string newAssigneeLabel);
    void RecordPriorityChanged(Ticket ticket, string changedByUserId, TicketPriority oldPriority, TicketPriority newPriority);
    void RecordTicketUpdated(Ticket ticket, string changedByUserId, IReadOnlyCollection<string> updates);
    void RecordCommentAdded(Ticket ticket, string changedByUserId, string commentContent);
}
