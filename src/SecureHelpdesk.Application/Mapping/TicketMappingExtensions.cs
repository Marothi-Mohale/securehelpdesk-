using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Mapping;

public static class TicketMappingExtensions
{
    public static TicketDetailDto ToDetailDto(this Ticket ticket)
    {
        return new TicketDetailDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAtUtc = ticket.CreatedAtUtc,
            UpdatedAtUtc = ticket.UpdatedAtUtc,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedByUser.FullName,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.FullName,
            Comments = ticket.Comments
                .OrderBy(c => c.CreatedAtUtc)
                .Select(c => new TicketCommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAtUtc = c.CreatedAtUtc,
                    UserId = c.AuthorUserId,
                    UserName = c.AuthorUser.FullName
                })
                .ToList(),
            AuditHistory = ticket.AuditLogs
                .OrderByDescending(a => a.ChangedAtUtc)
                .Select(a => new TicketAuditHistoryDto
                {
                    Id = a.Id,
                    ActionType = a.ActionType,
                    Description = BuildDescription(a),
                    CreatedAtUtc = a.ChangedAtUtc,
                    PerformedByUserId = a.ChangedByUserId,
                    PerformedByUserName = a.ChangedByUser.FullName
                })
                .ToList()
        };
    }

    private static string BuildDescription(TicketAuditLog auditLog)
    {
        return auditLog.ActionType switch
        {
            Domain.Enums.AuditActionType.TicketCreated => "Ticket created",
            Domain.Enums.AuditActionType.StatusChanged => $"Status changed from {auditLog.OldValue} to {auditLog.NewValue}",
            Domain.Enums.AuditActionType.AgentAssigned => $"Assigned agent changed from {auditLog.OldValue ?? "unassigned"} to {auditLog.NewValue ?? "unassigned"}",
            Domain.Enums.AuditActionType.CommentAdded => "Comment added",
            Domain.Enums.AuditActionType.PriorityChanged => $"Priority changed from {auditLog.OldValue} to {auditLog.NewValue}",
            _ => auditLog.ActionType.ToString()
        };
    }
}
