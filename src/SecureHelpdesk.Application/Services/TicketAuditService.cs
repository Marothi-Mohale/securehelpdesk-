using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Services;

public class TicketAuditService : ITicketAuditService
{
    public void RecordTicketCreated(Ticket ticket, string changedByUserId)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.TicketCreated,
            newValue: $"Status: {ticket.Status}; Priority: {ticket.Priority}"));
    }

    public void RecordStatusChanged(Ticket ticket, string changedByUserId, TicketStatus oldStatus, TicketStatus newStatus)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.StatusChanged,
            oldValue: oldStatus.ToString(),
            newValue: newStatus.ToString()));
    }

    public void RecordAssignmentChanged(Ticket ticket, string changedByUserId, string? oldAssigneeLabel, string newAssigneeLabel)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.AgentAssigned,
            oldValue: oldAssigneeLabel,
            newValue: newAssigneeLabel));
    }

    public void RecordPriorityChanged(Ticket ticket, string changedByUserId, TicketPriority oldPriority, TicketPriority newPriority)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.PriorityChanged,
            oldValue: oldPriority.ToString(),
            newValue: newPriority.ToString()));
    }

    public void RecordTicketUpdated(Ticket ticket, string changedByUserId, IReadOnlyCollection<string> updates)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.TicketUpdated,
            newValue: string.Join("; ", updates)));
    }

    public void RecordCommentAdded(Ticket ticket, string changedByUserId, string commentContent)
    {
        ticket.AuditLogs.Add(CreateAuditLog(
            ticketId: ticket.Id,
            changedByUserId: changedByUserId,
            actionType: AuditActionType.CommentAdded,
            newValue: BuildCommentPreview(commentContent)));
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
