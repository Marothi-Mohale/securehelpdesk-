using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Mapping;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketAuditService _ticketAuditService;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserDirectoryService _userDirectoryService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketAuditService ticketAuditService,
        ITicketRepository ticketRepository,
        IUserDirectoryService userDirectoryService,
        ILogger<TicketService> logger)
    {
        _ticketAuditService = ticketAuditService;
        _ticketRepository = ticketRepository;
        _userDirectoryService = userDirectoryService;
        _logger = logger;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = userContext.UserId
        };

        ticket.AuditLogs.Add(_ticketAuditService.RecordTicketCreated(ticket, userContext.UserId));

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} created by user {UserId}", ticket.Id, userContext.UserId);

        return await GetTicketByIdAsync(ticket.Id, userContext, cancellationToken);
    }

    public async Task<PaginatedResponseDto<TicketListResponseDto>> GetTicketsAsync(TicketQueryParameters queryParameters, UserContext userContext, CancellationToken cancellationToken)
    {
        var query = ApplySorting(
            ApplyFilters(BuildScopedQuery(userContext), queryParameters),
            queryParameters);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Select(t => new TicketListResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAtUtc = t.CreatedAtUtc,
                UpdatedAtUtc = t.UpdatedAtUtc,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByName = t.CreatedByUser.FullName,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CommentCount = t.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListResponseDto>
        {
            Items = items,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize,
            TotalCount = totalCount
        }.ToResponseDto();
    }

    public async Task<TicketResponseDto> GetTicketByIdAsync(Guid ticketId, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await GetRequiredTicketAsync(ticketId, cancellationToken);
        EnsureCanViewTicket(ticket, userContext);
        return ticket.ToResponseDto();
    }

    public async Task<TicketResponseDto> UpdateTicketAsync(Guid ticketId, UpdateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        if (request.Title is null && request.Description is null && request.Priority is null)
        {
            throw new ApiException("At least one editable field must be provided.", StatusCodes.Status400BadRequest);
        }

        var ticket = await GetRequiredTicketAsync(ticketId, cancellationToken);
        EnsureCanManageTicket(ticket, userContext);

        var updates = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var newTitle = request.Title.Trim();
            if (!string.Equals(ticket.Title, newTitle, StringComparison.Ordinal))
            {
                updates.Add("Title updated");
                ticket.Title = newTitle;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var newDescription = request.Description.Trim();
            if (!string.Equals(ticket.Description, newDescription, StringComparison.Ordinal))
            {
                updates.Add("Description updated");
                ticket.Description = newDescription;
            }
        }

        if (request.Priority.HasValue && ticket.Priority != request.Priority.Value)
        {
            await _ticketRepository.AddAuditLogAsync(
                _ticketAuditService.RecordPriorityChanged(ticket, userContext.UserId, ticket.Priority, request.Priority.Value),
                cancellationToken);
            updates.Add("Priority updated");
            ticket.Priority = request.Priority.Value;
        }

        if (updates.Count == 0)
        {
            throw new ApiException("No changes were detected.", StatusCodes.Status400BadRequest);
        }

        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordTicketUpdated(ticket, userContext.UserId, updates),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} details updated by user {UserId}", ticket.Id, userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketResponseDto> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await GetRequiredTicketAsync(ticketId, cancellationToken);
        EnsureCanManageTicket(ticket, userContext);
        var previousStatus = ticket.Status;

        if (previousStatus == request.Status)
        {
            throw new ApiException("Ticket already has the requested status.", StatusCodes.Status400BadRequest);
        }

        EnsureValidStatusTransition(previousStatus, request.Status);

        ticket.Status = request.Status;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordStatusChanged(ticket, userContext.UserId, previousStatus, request.Status),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketId} status changed from {PreviousStatus} to {NewStatus} by user {UserId}",
            ticket.Id,
            previousStatus,
            request.Status,
            userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketResponseDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        EnsureAdminRole(userContext);

        if (!await _userDirectoryService.UserExistsAsync(request.AgentUserId, cancellationToken))
        {
            throw new ApiException("Agent user was not found.", StatusCodes.Status404NotFound);
        }

        if (!await _userDirectoryService.UserIsInRoleAsync(request.AgentUserId, RoleNames.Agent, cancellationToken))
        {
            throw new ApiException("The selected assignee is not an agent.", StatusCodes.Status400BadRequest);
        }

        var ticket = await GetRequiredTicketAsync(ticketId, cancellationToken);
        var previousAssigneeUserId = ticket.AssignedToUserId;

        if (string.Equals(previousAssigneeUserId, request.AgentUserId, StringComparison.Ordinal))
        {
            throw new ApiException("Ticket is already assigned to the selected agent.", StatusCodes.Status400BadRequest);
        }

        var previousAssigneeLabel = await ResolveAssigneeLabelAsync(previousAssigneeUserId, cancellationToken);
        var newAssigneeLabel = await ResolveAssigneeLabelAsync(request.AgentUserId, cancellationToken)
            ?? request.AgentUserId;

        ticket.AssignedToUserId = request.AgentUserId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordAssignmentChanged(ticket, userContext.UserId, previousAssigneeLabel, newAssigneeLabel),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} assigned to {AgentUserId} by {UserId}", ticket.Id, request.AgentUserId, userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketResponseDto> AddCommentAsync(Guid ticketId, AddCommentRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await GetRequiredTicketAsync(ticketId, cancellationToken);
        EnsureCanViewTicket(ticket, userContext);
        var commentContent = request.Content.Trim();

        await _ticketRepository.AddCommentAsync(new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = userContext.UserId,
            Content = commentContent
        }, cancellationToken);

        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordCommentAdded(ticket, userContext.UserId, commentContent),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment added to ticket {TicketId} by user {UserId}", ticket.Id, userContext.UserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    private IQueryable<Ticket> BuildScopedQuery(UserContext userContext)
    {
        var query = _ticketRepository.QueryForList();

        if (userContext.IsAdmin)
        {
            return query;
        }

        if (userContext.IsAgent)
        {
            return query.Where(t => t.AssignedToUserId == userContext.UserId || t.CreatedByUserId == userContext.UserId);
        }

        return query.Where(t => t.CreatedByUserId == userContext.UserId);
    }

    private async Task<Ticket> GetRequiredTicketAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new ApiException("Ticket was not found.", StatusCodes.Status404NotFound);
        }

        return ticket;
    }

    private async Task<string?> ResolveAssigneeLabelAsync(string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var displayName = await _userDirectoryService.GetUserDisplayNameAsync(userId, cancellationToken);
        return $"{displayName} ({userId})";
    }

    private static IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, TicketQueryParameters queryParameters)
    {
        return (queryParameters.SortBy, queryParameters.SortDirection) switch
        {
            (TicketSortBy.UpdatedAt, SortDirection.Asc) => query.OrderBy(t => t.UpdatedAtUtc ?? t.CreatedAtUtc).ThenBy(t => t.CreatedAtUtc),
            (TicketSortBy.UpdatedAt, SortDirection.Desc) => query.OrderByDescending(t => t.UpdatedAtUtc ?? t.CreatedAtUtc).ThenByDescending(t => t.CreatedAtUtc),
            (TicketSortBy.CreatedAt, SortDirection.Asc) => query.OrderBy(t => t.CreatedAtUtc),
            _ => query.OrderByDescending(t => t.CreatedAtUtc)
        };
    }

    private static IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query, TicketQueryParameters queryParameters)
    {
        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            var search = queryParameters.Search.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Title, $"%{search}%") ||
                EF.Functions.Like(t.Description, $"%{search}%"));
        }

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

        if (!string.IsNullOrWhiteSpace(queryParameters.CreatedByUserId))
        {
            query = query.Where(t => t.CreatedByUserId == queryParameters.CreatedByUserId);
        }

        return query;
    }

    private static void EnsureCanViewTicket(Ticket ticket, UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return;
        }

        if (userContext.IsAgent)
        {
            if (string.Equals(ticket.AssignedToUserId, userContext.UserId, StringComparison.Ordinal)
                || string.Equals(ticket.CreatedByUserId, userContext.UserId, StringComparison.Ordinal))
            {
                return;
            }
        }
        else if (string.Equals(ticket.CreatedByUserId, userContext.UserId, StringComparison.Ordinal))
        {
            return;
        }

        throw new ApiException("You are not allowed to access this ticket.", StatusCodes.Status403Forbidden);
    }

    private static void EnsureCanManageTicket(Ticket ticket, UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return;
        }

        if (userContext.IsAgent && string.Equals(ticket.AssignedToUserId, userContext.UserId, StringComparison.Ordinal))
        {
            return;
        }

        throw new ApiException("You are not allowed to manage this ticket.", StatusCodes.Status403Forbidden);
    }

    private static void EnsureAdminRole(UserContext userContext)
    {
        if (!userContext.IsAdmin)
        {
            throw new ApiException("You are not allowed to perform this action.", StatusCodes.Status403Forbidden);
        }
    }

    private static void EnsureValidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        TicketStatus[] allowed = currentStatus switch
        {
            TicketStatus.Open => [TicketStatus.InProgress, TicketStatus.Closed],
            TicketStatus.InProgress => [TicketStatus.Resolved, TicketStatus.Closed],
            TicketStatus.Resolved => [TicketStatus.Closed, TicketStatus.InProgress],
            TicketStatus.Closed => [TicketStatus.InProgress],
            _ => Array.Empty<TicketStatus>()
        };

        if (!allowed.Contains(newStatus))
        {
            throw new ApiException(
                $"Status transition from {currentStatus} to {newStatus} is not allowed.",
                StatusCodes.Status400BadRequest);
        }
    }
}
