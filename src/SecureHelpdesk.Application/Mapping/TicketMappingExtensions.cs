using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Mapping;

public static class TicketMappingExtensions
{
    public static TicketResponseDto ToResponseDto(this Ticket ticket)
    {
        return new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAtUtc = ticket.CreatedAtUtc,
            UpdatedAtUtc = ticket.UpdatedAtUtc,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByName = ticket.CreatedByUser.FullName,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToName = ticket.AssignedToUser?.FullName,
            Comments = ticket.Comments
                .OrderBy(c => c.CreatedAtUtc)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAtUtc = c.CreatedAtUtc,
                    AuthorUserId = c.AuthorUserId,
                    AuthorName = c.AuthorUser.FullName
                })
                .ToList(),
            AuditLogs = ticket.AuditLogs
                .OrderByDescending(a => a.ChangedAtUtc)
                .Select(a => new AuditLogResponseDto
                {
                    Id = a.Id,
                    ActionType = a.ActionType,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    Description = BuildDescription(a),
                    ChangedAtUtc = a.ChangedAtUtc,
                    ChangedByUserId = a.ChangedByUserId,
                    ChangedByName = a.ChangedByUser.FullName
                })
                .ToList()
        };
    }

    public static TicketListResponseDto ToListResponseDto(this Ticket ticket)
    {
        return new TicketListResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAtUtc = ticket.CreatedAtUtc,
            UpdatedAtUtc = ticket.UpdatedAtUtc,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByName = ticket.CreatedByUser.FullName,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToName = ticket.AssignedToUser?.FullName,
            CommentCount = ticket.Comments.Count
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
