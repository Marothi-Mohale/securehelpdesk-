using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;
using SecureHelpdesk.Infrastructure.Persistence;

namespace SecureHelpdesk.Infrastructure.Seeding;

public class DbSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DbSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        foreach (var roleName in RoleNames.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var admin = await EnsureUserAsync("System Admin", "admin@securehelpdesk.local", "Admin123!", RoleNames.Admin);
        var agent1 = await EnsureUserAsync("Alice Agent", "agent1@securehelpdesk.local", "Agent123!", RoleNames.Agent);
        var agent2 = await EnsureUserAsync("Bob Agent", "agent2@securehelpdesk.local", "Agent123!", RoleNames.Agent);
        var user1 = await EnsureUserAsync("Uma User", "user1@securehelpdesk.local", "User123!", RoleNames.User);
        var user2 = await EnsureUserAsync("Ulysses User", "user2@securehelpdesk.local", "User123!", RoleNames.User);

        if (await _dbContext.Tickets.AnyAsync(cancellationToken))
        {
            return;
        }

        var firstTicket = new Ticket
        {
            Title = "Cannot access payroll portal",
            Description = "The payroll portal throws an unauthorized error after MFA verification.",
            Priority = TicketPriority.High,
            Status = TicketStatus.InProgress,
            CreatedByUserId = user1.Id,
            AssignedToUserId = agent1.Id,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        firstTicket.Comments.Add(new TicketComment
        {
            TicketId = firstTicket.Id,
            UserId = user1.Id,
            Content = "Issue started after my password reset.",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
        });

        firstTicket.Comments.Add(new TicketComment
        {
            TicketId = firstTicket.Id,
            UserId = agent1.Id,
            Content = "Investigating SSO token configuration for your account.",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        });

        firstTicket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = firstTicket.Id,
            PerformedByUserId = user1.Id,
            ActionType = AuditActionType.TicketCreated,
            Description = "Ticket created",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
        });

        firstTicket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = firstTicket.Id,
            PerformedByUserId = admin.Id,
            ActionType = AuditActionType.AgentAssigned,
            Description = $"Assigned to agent {agent1.FullName}",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        });

        firstTicket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = firstTicket.Id,
            PerformedByUserId = agent1.Id,
            ActionType = AuditActionType.StatusChanged,
            Description = $"Status changed from {TicketStatus.Open} to {TicketStatus.InProgress}",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        });

        var secondTicket = new Ticket
        {
            Title = "VPN client disconnects every 10 minutes",
            Description = "Remote desktop sessions are unstable because the VPN tunnel drops repeatedly.",
            Priority = TicketPriority.Critical,
            Status = TicketStatus.Open,
            CreatedByUserId = user2.Id,
            AssignedToUserId = agent2.Id,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        secondTicket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = secondTicket.Id,
            PerformedByUserId = user2.Id,
            ActionType = AuditActionType.TicketCreated,
            Description = "Ticket created",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        });

        secondTicket.AuditHistory.Add(new TicketAuditHistory
        {
            TicketId = secondTicket.Id,
            PerformedByUserId = admin.Id,
            ActionType = AuditActionType.AgentAssigned,
            Description = $"Assigned to agent {agent2.FullName}",
            CreatedAtUtc = DateTime.UtcNow.AddHours(-20)
        });

        _dbContext.Tickets.AddRange(firstTicket, secondTicket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded demo users and tickets");
    }

    private async Task<ApplicationUser> EnsureUserAsync(string fullName, string email, string password, string roleName)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errorText = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create seed user {email}: {errorText}");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        return user;
    }
}
