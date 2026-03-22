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
                    UserId = c.UserId,
                    UserName = c.User.FullName
                })
                .ToList(),
            AuditHistory = ticket.AuditHistory
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => new TicketAuditHistoryDto
                {
                    Id = a.Id,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    CreatedAtUtc = a.CreatedAtUtc,
                    PerformedByUserId = a.PerformedByUserId,
                    PerformedByUserName = a.PerformedByUser.FullName
                })
                .ToList()
        };
    }
}
