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
    private readonly ITicketAuthorizationService _ticketAuthorizationService;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserDirectoryService _userDirectoryService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketAuditService ticketAuditService,
        ITicketAuthorizationService ticketAuthorizationService,
        ITicketRepository ticketRepository,
        IUserDirectoryService userDirectoryService,
        ILogger<TicketService> logger)
    {
        _ticketAuditService = ticketAuditService;
        _ticketAuthorizationService = ticketAuthorizationService;
        _ticketRepository = ticketRepository;
        _userDirectoryService = userDirectoryService;
        _logger = logger;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["UserId"] = userContext.UserId
        });

        _ticketAuthorizationService.EnsureCanCreateTicket(userContext);

        var title = NormalizeRequiredText(request.Title, "Title", minimumLength: 5, maximumLength: 200);
        var description = NormalizeRequiredText(request.Description, "Description", minimumLength: 10, maximumLength: 4000);

        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = userContext.UserId
        };

        ticket.AuditLogs.Add(_ticketAuditService.RecordTicketCreated(ticket, userContext.UserId));

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketId} created with priority {Priority} and status {Status}",
            ticket.Id,
            ticket.Priority,
            ticket.Status);

        return await GetTicketByIdAsync(ticket.Id, userContext, cancellationToken);
    }

    public async Task<PaginatedResponseDto<TicketListResponseDto>> GetTicketsAsync(TicketQueryParameters queryParameters, UserContext userContext, CancellationToken cancellationToken)
    {
        var normalizedQuery = NormalizeQueryParameters(queryParameters);
        var query = ApplySorting(
            ApplyFilters(_ticketAuthorizationService.ApplyListScope(_ticketRepository.QueryForList(), userContext), normalizedQuery),
            normalizedQuery);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedQuery.PageNumber - 1) * normalizedQuery.PageSize)
            .Take(normalizedQuery.PageSize)
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
            PageNumber = normalizedQuery.PageNumber,
            PageSize = normalizedQuery.PageSize,
            TotalCount = totalCount
        }.ToResponseDto();
    }

    public async Task<TicketResponseDto> GetTicketByIdAsync(Guid ticketId, UserContext userContext, CancellationToken cancellationToken)
    {
        var ticket = await GetRequiredTicketAsync(ticketId, asNoTracking: true, cancellationToken);
        _ticketAuthorizationService.EnsureCanViewTicket(ticket, userContext);
        return ticket.ToResponseDto();
    }

    public async Task<TicketResponseDto> UpdateTicketAsync(Guid ticketId, UpdateTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TicketId"] = ticketId,
            ["UserId"] = userContext.UserId
        });

        if (request.Title is null && request.Description is null && request.Priority is null)
        {
            throw ApiException.Validation(
                "At least one editable field must be provided.",
                new Dictionary<string, string[]>
                {
                    ["request"] = ["At least one editable field must be provided."]
                });
        }

        var ticket = await GetRequiredTicketAsync(ticketId, asNoTracking: false, cancellationToken);
        _ticketAuthorizationService.EnsureCanManageTicket(ticket, userContext);

        var updates = new List<string>();

        if (request.Title is not null)
        {
            var newTitle = NormalizeRequiredText(request.Title, "Title", minimumLength: 5, maximumLength: 200);
            if (!string.Equals(ticket.Title, newTitle, StringComparison.Ordinal))
            {
                updates.Add("Title updated");
                ticket.Title = newTitle;
            }
        }

        if (request.Description is not null)
        {
            var newDescription = NormalizeRequiredText(request.Description, "Description", minimumLength: 10, maximumLength: 4000);
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
            throw ApiException.BadRequest("No changes were detected.", ErrorCodes.TicketNoChangesDetected);
        }

        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordTicketUpdated(ticket, userContext.UserId, updates),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket details updated. Changes: {Updates}", string.Join(", ", updates));

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketResponseDto> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TicketId"] = ticketId,
            ["UserId"] = userContext.UserId
        });

        var ticket = await GetRequiredTicketAsync(ticketId, asNoTracking: false, cancellationToken);
        _ticketAuthorizationService.EnsureCanManageTicket(ticket, userContext);
        var previousStatus = ticket.Status;

        if (previousStatus == request.Status)
        {
            throw ApiException.BadRequest("Ticket already has the requested status.", ErrorCodes.TicketNoChangesDetected);
        }

        TicketWorkflowRules.EnsureValidStatusTransition(previousStatus, request.Status);

        ticket.Status = request.Status;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordStatusChanged(ticket, userContext.UserId, previousStatus, request.Status),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket status changed from {PreviousStatus} to {NewStatus}",
            previousStatus,
            request.Status);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<TicketResponseDto> AssignTicketAsync(Guid ticketId, AssignTicketRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TicketId"] = ticketId,
            ["UserId"] = userContext.UserId
        });

        _ticketAuthorizationService.EnsureCanAssignTicket(userContext);
        var agentUserId = NormalizeRequiredText(request.AgentUserId, nameof(request.AgentUserId), minimumLength: 1, maximumLength: 450);

        if (!await _userDirectoryService.UserExistsAsync(agentUserId, cancellationToken))
        {
            throw ApiException.NotFound("Agent user was not found.", ErrorCodes.UserNotFound);
        }

        if (!await _userDirectoryService.UserIsInRoleAsync(agentUserId, RoleNames.Agent, cancellationToken))
        {
            throw ApiException.BadRequest("The selected assignee is not an agent.", ErrorCodes.UserNotAgent);
        }

        var ticket = await GetRequiredTicketAsync(ticketId, asNoTracking: false, cancellationToken);
        var previousAssigneeUserId = ticket.AssignedToUserId;

        if (string.Equals(previousAssigneeUserId, agentUserId, StringComparison.Ordinal))
        {
            throw ApiException.BadRequest("Ticket is already assigned to the selected agent.", ErrorCodes.TicketAlreadyAssigned);
        }

        var previousAssigneeLabel = await ResolveAssigneeLabelAsync(previousAssigneeUserId, cancellationToken);
        var newAssigneeLabel = await ResolveAssigneeLabelAsync(agentUserId, cancellationToken)
            ?? agentUserId;

        ticket.AssignedToUserId = agentUserId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordAssignmentChanged(ticket, userContext.UserId, previousAssigneeLabel, newAssigneeLabel),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket assigned to agent {AgentUserId}", agentUserId);

        return await GetTicketByIdAsync(ticketId, userContext, cancellationToken);
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid ticketId, AddCommentRequestDto request, UserContext userContext, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["TicketId"] = ticketId,
            ["UserId"] = userContext.UserId
        });

        var ticket = await GetRequiredTicketAsync(ticketId, asNoTracking: false, cancellationToken);
        _ticketAuthorizationService.EnsureCanViewTicket(ticket, userContext);
        var commentContent = NormalizeRequiredText(request.Content, nameof(request.Content), minimumLength: 1, maximumLength: 1000);

        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = userContext.UserId,
            Content = commentContent
        };

        await _ticketRepository.AddCommentAsync(comment, cancellationToken);

        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await _ticketRepository.AddAuditLogAsync(
            _ticketAuditService.RecordCommentAdded(ticket, userContext.UserId, commentContent),
            cancellationToken);

        await _ticketRepository.SaveChangesAsync(cancellationToken);

        var authorName = await _userDirectoryService.GetUserDisplayNameAsync(userContext.UserId, cancellationToken);

        _logger.LogInformation("Comment added to ticket with content length {CommentLength}", commentContent.Length);

        return comment.ToResponseDto(authorName);
    }

    private async Task<Ticket> GetRequiredTicketAsync(Guid ticketId, bool asNoTracking, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(ticketId, asNoTracking, cancellationToken);

        if (ticket is null)
        {
            throw ApiException.NotFound("Ticket was not found.", ErrorCodes.TicketNotFound);
        }

        return ticket;
    }

    private static TicketQueryParameters NormalizeQueryParameters(TicketQueryParameters queryParameters)
    {
        return new TicketQueryParameters
        {
            Search = string.IsNullOrWhiteSpace(queryParameters.Search) ? null : queryParameters.Search.Trim(),
            Status = queryParameters.Status,
            Priority = queryParameters.Priority,
            AssignedToUserId = NormalizeOptionalIdentifier(queryParameters.AssignedToUserId),
            CreatedByUserId = NormalizeOptionalIdentifier(queryParameters.CreatedByUserId),
            SortBy = queryParameters.SortBy,
            SortDirection = queryParameters.SortDirection,
            PageNumber = queryParameters.PageNumber,
            PageSize = queryParameters.PageSize
        };
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

    private static string? NormalizeOptionalIdentifier(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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

        if (!string.IsNullOrWhiteSpace(queryParameters.AssignedToUserId))
        {
            query = query.Where(t => t.AssignedToUserId == queryParameters.AssignedToUserId);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.CreatedByUserId))
        {
            query = query.Where(t => t.CreatedByUserId == queryParameters.CreatedByUserId);
        }

        return query;
    }

    private static string NormalizeRequiredText(string? input, string fieldName, int minimumLength, int maximumLength)
    {
        var normalized = input?.Trim() ?? string.Empty;

        if (normalized.Length < minimumLength || normalized.Length > maximumLength)
        {
            throw ApiException.Validation(
                $"{fieldName} must be between {minimumLength} and {maximumLength} characters after trimming.",
                new Dictionary<string, string[]>
                {
                    [fieldName] = [$"{fieldName} must be between {minimumLength} and {maximumLength} characters after trimming."]
                });
        }

        return normalized;
    }
}
