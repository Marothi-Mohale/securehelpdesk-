using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserDirectoryService _userDirectoryService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        IUserDirectoryService userDirectoryService,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _userDirectoryService = userDirectoryService;
        _logger = logger;
    }

    public async Task<TicketDetailDto> CreateTicketAsync(CreateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = userContext.UserId
        };

        ticket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = ticket.Id,
            PerformedByUserId = userContext.UserId,
            ActionType = AuditActionType.TicketCreated,
            Description = "Ticket created"
        });

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} created by user {UserId}", ticket.Id, userContext.UserId);

        return await GetTicketByIdAsync(ticket.Id, userContext, cancellationToken);
    }

    public async Task<PagedResult<TicketListItemDto>> GetTicketsAsync(TicketQueryParameters queryParameters, UserContext userContext, CancellationToken cancellationToken)
    {
        var query = BuildScopedQuery(userContext);

        if (queryParameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == queryParameters.Status.Value);
        }

        if (queryParameters.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == queryParameters.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.AssigneeId))
        {
            query = query.Where(t => t.AssignedToUserId == queryParameters.AssigneeId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAtUtc = t.CreatedAtUtc,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.FullName,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CommentCount = t.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Items = items,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TicketDetailDto> GetTicketByIdAsync(Guid ticketId, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.AuditHistory)
                .ThenInclude(a => a.PerformedByUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new ApiException("Ticket was not found.", StatusCodes.Status404NotFound);
        }

        EnsureTicketAccess(ticket, userContext);
        return MapDetail(ticket);
    }

    public async Task<TicketDetailDto> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        EnsureSupportRole(userContext);

        var ticket = await GetTrackedTicketAsync(ticketId, cancellationToken);
        var previousStatus = ticket.Status;

        if (previousStatus == request.Status)
        {
            throw new ApiException("Ticket already has the requested status.", StatusCodes.Status400BadRequest);
        }

        ticket.Status = request.Status;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        ticket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = ticket.Id,
            PerformedByUserId = userContext.UserId,
            ActionType = AuditActionType.StatusChanged,
            Description = $"Status changed from {previousStatus} to {request.Status}"
        });

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketId} status changed from {PreviousStatus} to {NewStatus} by user {UserId}",
            ticket.Id,
            previousStatus,
            request.Status,
            userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketDetailDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        EnsureSupportRole(userContext);

        if (!await _userDirectoryService.UserExistsAsync(request.AgentUserId, cancellationToken))
        {
            throw new ApiException("Agent user was not found.", StatusCodes.Status404NotFound);
        }

        if (!await _userDirectoryService.UserIsInRoleAsync(request.AgentUserId, RoleNames.Agent, cancellationToken))
        {
            throw new ApiException("The selected assignee is not an agent.", StatusCodes.Status400BadRequest);
        }

        var ticket = await GetTrackedTicketAsync(ticketId, cancellationToken);
        ticket.AssignedToUserId = request.AgentUserId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        var assigneeName = await _userDirectoryService.GetUserDisplayNameAsync(request.AgentUserId, cancellationToken);
        ticket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = ticket.Id,
            PerformedByUserId = userContext.UserId,
            ActionType = AuditActionType.AgentAssigned,
            Description = $"Assigned to agent {assigneeName}"
        });

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} assigned to {AgentUserId} by {UserId}", ticket.Id, request.AgentUserId, userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketDetailDto> AddCommentAsync(Guid ticketId, AddCommentRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await GetTrackedTicketAsync(ticketId, cancellationToken);
        EnsureTicketAccess(ticket, userContext);

        ticket.Comments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            UserId = userContext.UserId,
            Content = request.Content.Trim()
        });

        ticket.UpdatedAtUtc = DateTime.UtcNow;
        ticket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = ticket.Id,
            PerformedByUserId = userContext.UserId,
            ActionType = AuditActionType.CommentAdded,
            Description = "Comment added"
        });

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment added to ticket {TicketId} by user {UserId}", ticket.Id, userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    private IQueryable<Ticket> BuildScopedQuery(UserContext userContext)
    {
        var query = _ticketRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .AsQueryable();

        if (userContext.IsAdmin || userContext.IsAgent)
        {
            return query;
        }

        return query.Where(t => t.CreatedByUserId == userContext.UserId);
    }

    private async Task<Ticket> GetTrackedTicketAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.Query()
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.AuditHistory)
                .ThenInclude(a => a.PerformedByUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new ApiException("Ticket was not found.", StatusCodes.Status404NotFound);
        }

        return ticket;
    }

    private static void EnsureSupportRole(UserContext userContext)
    {
        if (!userContext.IsAdmin && !userContext.IsAgent)
        {
            throw new ApiException("You are not allowed to perform this action.", StatusCodes.Status403Forbidden);
        }
    }

    private static void EnsureTicketAccess(Ticket ticket, UserContext userContext)
    {
        if (userContext.IsAdmin || userContext.IsAgent)
        {
            return;
        }

        if (!string.Equals(ticket.CreatedByUserId, userContext.UserId, StringComparison.Ordinal))
        {
            throw new ApiException("You are not allowed to access this ticket.", StatusCodes.Status403Forbidden);
        }
    }

    private static TicketDetailDto MapDetail(Ticket ticket)
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
