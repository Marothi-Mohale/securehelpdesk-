using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Models;
using SecureHelpdesk.Application.Services;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;
using SecureHelpdesk.Infrastructure.Data;
using SecureHelpdesk.Infrastructure.Repositories;
using Xunit;

namespace SecureHelpdesk.Tests;

public class TicketServiceTests
{
    private readonly Mock<IUserDirectoryService> _userDirectoryService = new();
    private readonly Mock<ILogger<TicketService>> _logger = new();
    private readonly ITicketAuditService _ticketAuditService = new TicketAuditService();

    [Fact]
    public async Task AssignTicketAsync_Throws_When_Assignee_Is_Not_Agent()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");

        _userDirectoryService.Setup(s => s.UserExistsAsync("user-2", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userDirectoryService.Setup(s => s.UserIsInRoleAsync("user-2", RoleNames.Agent, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var service = CreateService(dbContext);
        var context = new UserContext("admin-1", "admin@test.local", [RoleNames.Admin]);

        var act = async () => await service.AssignTicketAsync(ticket.Id, new AssignTicketRequestDto { AgentUserId = "user-2" }, context, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task GetTicketsAsync_For_Regular_User_Returns_Only_Own_Tickets()
    {
        await using var dbContext = CreateDbContext();
        await SeedTicketAsync(dbContext, "user-1");
        await SeedTicketAsync(dbContext, "user-2");

        var service = CreateService(dbContext);
        var result = await service.GetTicketsAsync(new TicketQueryParameters(), new UserContext("user-1", "user1@test.local", [RoleNames.User]), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("user-1", result.Items.Single().CreatedByUserId);
    }

    [Fact]
    public async Task CreateTicketAsync_Creates_Open_Ticket_With_Audit_Log()
    {
        await using var dbContext = CreateDbContext();
        await EnsureUserAsync(dbContext, "user-1", "user1@test.local", "User One");

        var service = CreateService(dbContext);
        var context = new UserContext("user-1", "user1@test.local", [RoleNames.User]);

        var result = await service.CreateTicketAsync(
            new CreateTicketRequestDto
            {
                Title = "  Printer queue stuck  ",
                Description = "  Print jobs remain in the queue and never reach the device.  ",
                Priority = TicketPriority.High
            },
            context,
            CancellationToken.None);

        Assert.Equal("Printer queue stuck", result.Title);
        Assert.Equal("Print jobs remain in the queue and never reach the device.", result.Description);
        Assert.Equal(TicketStatus.Open, result.Status);
        Assert.Equal(TicketPriority.High, result.Priority);
        Assert.Equal("user-1", result.CreatedByUserId);
        Assert.Contains(result.AuditLogs, log => log.ActionType == AuditActionType.TicketCreated);
    }

    [Fact]
    public async Task CreateTicketAsync_Throws_When_Trimmed_Text_Is_Invalid()
    {
        await using var dbContext = CreateDbContext();
        await EnsureUserAsync(dbContext, "user-1", "user1@test.local", "User One");

        var service = CreateService(dbContext);
        var context = new UserContext("user-1", "user1@test.local", [RoleNames.User]);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateTicketAsync(
            new CreateTicketRequestDto
            {
                Title = "     ",
                Description = "          ",
                Priority = TicketPriority.Low
            },
            context,
            CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_When_Role_Is_Not_Support()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");

        var service = CreateService(dbContext);
        var context = new UserContext("user-1", "user1@test.local", [RoleNames.User]);

        var act = async () => await service.UpdateStatusAsync(ticket.Id, new UpdateTicketStatusRequestDto { Status = TicketStatus.Resolved }, context, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(403, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_When_Transition_Is_Invalid()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1", assignedToUserId: "agent-1");

        var service = CreateService(dbContext);
        var context = new UserContext("agent-1", "agent1@test.local", [RoleNames.Agent]);

        var act = async () => await service.UpdateStatusAsync(
            ticket.Id,
            new UpdateTicketStatusRequestDto { Status = TicketStatus.Resolved },
            context,
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task AssignTicketAsync_Throws_When_User_Is_Not_Admin()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");

        _userDirectoryService.Setup(s => s.UserExistsAsync("agent-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userDirectoryService.Setup(s => s.UserIsInRoleAsync("agent-1", RoleNames.Agent, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = CreateService(dbContext);
        var context = new UserContext("agent-2", "agent@test.local", [RoleNames.Agent]);

        var act = async () => await service.AssignTicketAsync(ticket.Id, new AssignTicketRequestDto { AgentUserId = "agent-1" }, context, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(403, exception.StatusCode);
    }

    [Fact]
    public async Task AssignTicketAsync_Updates_Assignee_And_Creates_Audit_Log()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");
        await EnsureUserAsync(dbContext, "admin-1", "admin@test.local", "System Admin");
        await EnsureUserAsync(dbContext, "agent-1", "agent1@test.local", "Alice Agent");

        _userDirectoryService.Setup(s => s.UserExistsAsync("agent-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userDirectoryService.Setup(s => s.UserIsInRoleAsync("agent-1", RoleNames.Agent, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userDirectoryService.Setup(s => s.GetUserDisplayNameAsync("agent-1", It.IsAny<CancellationToken>())).ReturnsAsync("Alice Agent");

        var service = CreateService(dbContext);
        var context = new UserContext("admin-1", "admin@test.local", [RoleNames.Admin]);

        var result = await service.AssignTicketAsync(
            ticket.Id,
            new AssignTicketRequestDto { AgentUserId = "agent-1" },
            context,
            CancellationToken.None);

        Assert.Equal("agent-1", result.AssignedToUserId);
        Assert.Contains(result.AuditLogs, log =>
            log.ActionType == AuditActionType.AgentAssigned &&
            log.NewValue == "Alice Agent (agent-1)");
    }

    [Fact]
    public async Task UpdateTicketAsync_Throws_When_Agent_Is_Not_Assigned()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");

        var service = CreateService(dbContext);
        var context = new UserContext("agent-1", "agent1@test.local", [RoleNames.Agent]);

        var act = async () => await service.UpdateTicketAsync(
            ticket.Id,
            new UpdateTicketRequestDto { Title = "Updated title" },
            context,
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(403, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateTicketAsync_Updates_Priority_And_Writes_Audit_Entries()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1", assignedToUserId: "agent-1");

        var service = CreateService(dbContext);
        var context = new UserContext("agent-1", "agent1@test.local", [RoleNames.Agent]);

        var result = await service.UpdateTicketAsync(
            ticket.Id,
            new UpdateTicketRequestDto
            {
                Title = "Updated seeded ticket title",
                Priority = TicketPriority.Critical
            },
            context,
            CancellationToken.None);

        Assert.Equal("Updated seeded ticket title", result.Title);
        Assert.Equal(TicketPriority.Critical, result.Priority);
        Assert.NotNull(result.UpdatedAtUtc);
        Assert.Contains(result.AuditLogs, log => log.ActionType == AuditActionType.PriorityChanged);
        Assert.Contains(result.AuditLogs, log => log.ActionType == AuditActionType.TicketUpdated);
    }

    [Fact]
    public async Task AddCommentAsync_Adds_Comment_And_Audit_Log_For_Authorized_User()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");
        _userDirectoryService
            .Setup(service => service.GetUserDisplayNameAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("User user-1");

        var service = CreateService(dbContext);
        var context = new UserContext("user-1", "user1@test.local", [RoleNames.User]);

        var result = await service.AddCommentAsync(
            ticket.Id,
            new AddCommentRequestDto { Content = "Need help with this issue." },
            context,
            CancellationToken.None);

        Assert.Equal("Need help with this issue.", result.Content);
        Assert.Equal("user-1", result.AuthorUserId);
        Assert.Equal("User user-1", result.AuthorName);

        var reloadedTicket = await dbContext.Tickets
            .Include(t => t.AuditLogs)
            .Include(t => t.Comments)
            .SingleAsync(t => t.Id == ticket.Id);

        Assert.Single(reloadedTicket.Comments);
        Assert.Contains(reloadedTicket.AuditLogs, log => log.ActionType == AuditActionType.CommentAdded);
    }

    [Fact]
    public async Task AddCommentAsync_Throws_When_Content_Is_Whitespace()
    {
        await using var dbContext = CreateDbContext();
        var ticket = await SeedTicketAsync(dbContext, "user-1");

        var service = CreateService(dbContext);
        var context = new UserContext("user-1", "user1@test.local", [RoleNames.User]);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.AddCommentAsync(
            ticket.Id,
            new AddCommentRequestDto { Content = "   " },
            context,
            CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task GetTicketsAsync_Applies_Search_Status_And_Sorting()
    {
        await using var dbContext = CreateDbContext();

        var olderTicket = await SeedTicketAsync(
            dbContext,
            "user-1",
            title: "VPN client disconnects",
            description: "VPN drops during large file transfers.",
            status: TicketStatus.Open,
            priority: TicketPriority.High,
            createdAtUtc: DateTime.UtcNow.AddDays(-2));

        var newerTicket = await SeedTicketAsync(
            dbContext,
            "user-1",
            title: "VPN client disconnects on Wi-Fi",
            description: "VPN drops after five minutes on wireless.",
            status: TicketStatus.Open,
            priority: TicketPriority.High,
            createdAtUtc: DateTime.UtcNow.AddHours(-1));

        await SeedTicketAsync(
            dbContext,
            "user-1",
            title: "Printer issue",
            description: "Printer is offline.",
            status: TicketStatus.Closed,
            priority: TicketPriority.Low,
            createdAtUtc: DateTime.UtcNow.AddHours(-3));

        var service = CreateService(dbContext);
        var result = await service.GetTicketsAsync(
            new TicketQueryParameters
            {
                Search = "VPN",
                Status = TicketStatus.Open,
                SortBy = TicketSortBy.CreatedAt,
                SortDirection = SortDirection.Desc
            },
            new UserContext("user-1", "user1@test.local", [RoleNames.User]),
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(newerTicket.Id, result.Items.First().Id);
        Assert.Equal(olderTicket.Id, result.Items.Last().Id);
    }

    private TicketService CreateService(ApplicationDbContext dbContext)
    {
        return new TicketService(_ticketAuditService, new TicketRepository(dbContext), _userDirectoryService.Object, _logger.Object);
    }

    private static async Task<Ticket> SeedTicketAsync(
        ApplicationDbContext dbContext,
        string createdByUserId,
        string? assignedToUserId = null,
        string? title = null,
        string? description = null,
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Medium,
        DateTime? createdAtUtc = null)
    {
        var creator = await EnsureUserAsync(dbContext, createdByUserId, $"{createdByUserId}@test.local", $"User {createdByUserId}");
        ApplicationUser? assignee = null;
        if (!string.IsNullOrWhiteSpace(assignedToUserId))
        {
            assignee = await EnsureUserAsync(dbContext, assignedToUserId, $"{assignedToUserId}@test.local", $"User {assignedToUserId}");
        }

        var ticket = new Ticket
        {
            Title = title ?? $"Ticket for {createdByUserId}",
            Description = description ?? "Test description for seeded ticket.",
            CreatedByUserId = creator.Id,
            CreatedByUser = creator,
            AssignedToUserId = assignee?.Id,
            AssignedToUser = assignee,
            Status = status,
            Priority = priority,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        };

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return ticket;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(ApplicationDbContext dbContext, string id, string email, string fullName)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is not null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
