using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Services;

public class TicketAuditService : ITicketAuditService
{
    public TicketAuditLog RecordTicketCreated(Ticket ticket, string changedByUserId)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.TicketCreated,
            newValue: $"Status: {ticket.Status}; Priority: {ticket.Priority}");
    }

    public TicketAuditLog RecordStatusChanged(Ticket ticket, string changedByUserId, TicketStatus oldStatus, TicketStatus newStatus)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.StatusChanged,
            oldValue: oldStatus.ToString(),
            newValue: newStatus.ToString());
    }

    public TicketAuditLog RecordAssignmentChanged(Ticket ticket, string changedByUserId, string? oldAssigneeLabel, string newAssigneeLabel)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.AgentAssigned,
            oldValue: oldAssigneeLabel,
            newValue: newAssigneeLabel);
    }

    public TicketAuditLog RecordPriorityChanged(Ticket ticket, string changedByUserId, TicketPriority oldPriority, TicketPriority newPriority)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.PriorityChanged,
            oldValue: oldPriority.ToString(),
            newValue: newPriority.ToString());
    }

    public TicketAuditLog RecordTicketUpdated(Ticket ticket, string changedByUserId, IReadOnlyCollection<string> updates)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.TicketUpdated,
            newValue: string.Join("; ", updates));
    }

    public TicketAuditLog RecordCommentAdded(Ticket ticket, string changedByUserId, string commentContent)
    {
        return CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.CommentAdded,
            newValue: BuildCommentPreview(commentContent));
    }

    private static TicketAuditLog CreateAuditLog(
        Guid ticketId,
        string changedByUserId,
        AuditActionType actionType,
        string? oldValue = null,
        string? newValue = null)
    {
        return new TicketAuditLog
        {
            TicketId = ticketId,
            ChangedByUserId = changedByUserId,
            ActionType = actionType,
            OldValue = oldValue,
            NewValue = newValue
        };
    }

    private static string BuildCommentPreview(string commentContent)
    {
        var trimmed = commentContent.Trim();
        const int maxLength = 80;

        if (trimmed.Length <= maxLength)
        {
            return trimmed;
        }

        return $"{trimmed[..maxLength]}...";
    }
}
