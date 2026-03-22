using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SecureHelpdesk.Application;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Application.Services;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;
using SecureHelpdesk.Infrastructure.Persistence;
using Xunit;

namespace SecureHelpdesk.Tests;

public class TicketServiceTests
{
    private readonly Mock<IUserDirectoryService> _userDirectoryService = new();
    private readonly Mock<ILogger<TicketService>> _logger = new();

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

    private TicketService CreateService(ApplicationDbContext dbContext)
    {
        return new TicketService(new TicketRepository(dbContext), _userDirectoryService.Object, _logger.Object);
    }

    private static async Task<Ticket> SeedTicketAsync(ApplicationDbContext dbContext, string createdByUserId)
    {
        var creator = await EnsureUserAsync(dbContext, createdByUserId, $"{createdByUserId}@test.local", $"User {createdByUserId}");

        var ticket = new Ticket
        {
            Title = $"Ticket for {createdByUserId}",
            Description = "Test description for seeded ticket.",
            CreatedByUserId = creator.Id,
            CreatedByUser = creator,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium
        };

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();
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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
