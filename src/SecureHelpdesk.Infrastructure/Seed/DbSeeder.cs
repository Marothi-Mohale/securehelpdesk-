using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;
using SecureHelpdesk.Infrastructure.Data;

namespace SecureHelpdesk.Infrastructure.Seed;

public class DbSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SeedOptions _seedOptions;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SeedOptions> seedOptions,
        ILogger<DbSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _seedOptions = seedOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseAsync(cancellationToken);
        await EnsureRolesAsync();

        var users = await EnsureUsersAsync();

        if (await _dbContext.Tickets.AnyAsync(cancellationToken))
        {
            _logger.LogInformation(
                "Skipping demo ticket seeding because ticket data already exists. Existing ticket count: {TicketCount}",
                await _dbContext.Tickets.CountAsync(cancellationToken));

            return;
        }

        var seededTickets = BuildDemoTickets(users);
        _dbContext.Tickets.AddRange(seededTickets);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var ticketCount = seededTickets.Count;
        var commentCount = seededTickets.Sum(ticket => ticket.Comments.Count);
        var auditLogCount = seededTickets.Sum(ticket => ticket.AuditLogs.Count);

        _logger.LogInformation(
            "Seeded demo data with roles {RoleCount}, users {UserCount}, tickets {TicketCount}, comments {CommentCount}, and audit logs {AuditLogCount}",
            RoleNames.All.Length,
            users.Count,
            ticketCount,
            commentCount,
            auditLogCount);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        var availableMigrations = _dbContext.Database.GetMigrations().ToList();

        if (availableMigrations.Count > 0)
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
            return;
        }

        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var roleName in RoleNames.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errorText = string.Join("; ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Failed to create seed role {roleName}: {errorText}");
                }
            }
        }
    }

    private async Task<Dictionary<string, ApplicationUser>> EnsureUsersAsync()
    {
        var admin = await EnsureUserAsync("System Admin", "admin@securehelpdesk.local", GetPassword(RoleNames.Admin), RoleNames.Admin);
        var agent1 = await EnsureUserAsync("Alice Agent", "agent1@securehelpdesk.local", GetPassword(RoleNames.Agent), RoleNames.Agent);
        var agent2 = await EnsureUserAsync("Bob Agent", "agent2@securehelpdesk.local", GetPassword(RoleNames.Agent), RoleNames.Agent);
        var user1 = await EnsureUserAsync("Uma User", "user1@securehelpdesk.local", GetPassword(RoleNames.User), RoleNames.User);
        var user2 = await EnsureUserAsync("Ulysses User", "user2@securehelpdesk.local", GetPassword(RoleNames.User), RoleNames.User);
        var user3 = await EnsureUserAsync("Nina User", "user3@securehelpdesk.local", GetPassword(RoleNames.User), RoleNames.User);

        return new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = admin,
            ["agent1"] = agent1,
            ["agent2"] = agent2,
            ["user1"] = user1,
            ["user2"] = user2,
            ["user3"] = user3
        };
    }

    private async Task<ApplicationUser> EnsureUserAsync(string fullName, string email, string password, string roleName)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

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
                var errorText = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to create seed user {email}: {errorText}");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addToRoleResult.Succeeded)
            {
                var errorText = string.Join("; ", addToRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to add seed user {email} to role {roleName}: {errorText}");
            }
        }

        return user;
    }

    private List<Ticket> BuildDemoTickets(IReadOnlyDictionary<string, ApplicationUser> users)
    {
        var now = DateTime.UtcNow;

        var firstTicket = CreateTicket(
            title: "Cannot access payroll portal",
            description: "The payroll portal throws an unauthorized error after MFA verification.",
            priority: TicketPriority.High,
            status: TicketStatus.InProgress,
            createdByUserId: users["user1"].Id,
            assignedToUserId: users["agent1"].Id,
            createdAtUtc: now.AddDays(-4),
            updatedAtUtc: now.AddDays(-2));

        AddComment(firstTicket, users["user1"].Id, "Issue started immediately after my password reset.", now.AddDays(-4));
        AddComment(firstTicket, users["agent1"].Id, "I am checking whether the SSO claim mapping was reset for your profile.", now.AddDays(-3));
        AddAuditLog(firstTicket, users["user1"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddDays(-4));
        AddAuditLog(firstTicket, users["admin"].Id, AuditActionType.AgentAssigned, null, $"{users["agent1"].FullName} ({users["agent1"].Id})", now.AddDays(-3));
        AddAuditLog(firstTicket, users["agent1"].Id, AuditActionType.StatusChanged, TicketStatus.Open.ToString(), TicketStatus.InProgress.ToString(), now.AddDays(-2));

        var secondTicket = CreateTicket(
            title: "VPN client disconnects every 10 minutes",
            description: "Remote desktop sessions are unstable because the VPN tunnel drops repeatedly during large file transfers.",
            priority: TicketPriority.Critical,
            status: TicketStatus.Open,
            createdByUserId: users["user2"].Id,
            assignedToUserId: users["agent2"].Id,
            createdAtUtc: now.AddDays(-2),
            updatedAtUtc: now.AddHours(-18));

        AddComment(secondTicket, users["user2"].Id, "This affects all remote work after around ten minutes.", now.AddDays(-2));
        AddAuditLog(secondTicket, users["user2"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddDays(-2));
        AddAuditLog(secondTicket, users["admin"].Id, AuditActionType.AgentAssigned, null, $"{users["agent2"].FullName} ({users["agent2"].Id})", now.AddDays(-1));

        var thirdTicket = CreateTicket(
            title: "Laptop battery drains rapidly after update",
            description: "Battery life dropped from six hours to under ninety minutes after the latest corporate image update.",
            priority: TicketPriority.Medium,
            status: TicketStatus.Resolved,
            createdByUserId: users["user3"].Id,
            assignedToUserId: users["agent1"].Id,
            createdAtUtc: now.AddDays(-8),
            updatedAtUtc: now.AddDays(-5));

        AddComment(thirdTicket, users["user3"].Id, "The fan also runs constantly even while idle.", now.AddDays(-8));
        AddComment(thirdTicket, users["agent1"].Id, "Power profile was reset and unnecessary startup apps were removed.", now.AddDays(-6));
        AddAuditLog(thirdTicket, users["user3"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddDays(-8));
        AddAuditLog(thirdTicket, users["admin"].Id, AuditActionType.AgentAssigned, null, $"{users["agent1"].FullName} ({users["agent1"].Id})", now.AddDays(-7));
        AddAuditLog(thirdTicket, users["agent1"].Id, AuditActionType.StatusChanged, TicketStatus.Open.ToString(), TicketStatus.InProgress.ToString(), now.AddDays(-7));
        AddAuditLog(thirdTicket, users["agent1"].Id, AuditActionType.StatusChanged, TicketStatus.InProgress.ToString(), TicketStatus.Resolved.ToString(), now.AddDays(-5));

        var fourthTicket = CreateTicket(
            title: "Shared printer on floor 3 is offline",
            description: "The printer used by finance cannot be reached from any workstation on the third floor.",
            priority: TicketPriority.Low,
            status: TicketStatus.Closed,
            createdByUserId: users["user1"].Id,
            assignedToUserId: users["agent2"].Id,
            createdAtUtc: now.AddDays(-12),
            updatedAtUtc: now.AddDays(-9));

        AddComment(fourthTicket, users["user1"].Id, "The display on the printer shows a network error.", now.AddDays(-12));
        AddComment(fourthTicket, users["agent2"].Id, "Printer NIC was reseated and the print server queue was restarted.", now.AddDays(-10));
        AddAuditLog(fourthTicket, users["user1"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddDays(-12));
        AddAuditLog(fourthTicket, users["admin"].Id, AuditActionType.AgentAssigned, null, $"{users["agent2"].FullName} ({users["agent2"].Id})", now.AddDays(-11));
        AddAuditLog(fourthTicket, users["agent2"].Id, AuditActionType.StatusChanged, TicketStatus.Open.ToString(), TicketStatus.InProgress.ToString(), now.AddDays(-11));
        AddAuditLog(fourthTicket, users["agent2"].Id, AuditActionType.StatusChanged, TicketStatus.InProgress.ToString(), TicketStatus.Resolved.ToString(), now.AddDays(-10));
        AddAuditLog(fourthTicket, users["agent2"].Id, AuditActionType.StatusChanged, TicketStatus.Resolved.ToString(), TicketStatus.Closed.ToString(), now.AddDays(-9));

        var fifthTicket = CreateTicket(
            title: "Onboarding account missing CRM access",
            description: "A new sales employee can sign in, but the CRM tile is missing from the company app launcher.",
            priority: TicketPriority.High,
            status: TicketStatus.Open,
            createdByUserId: users["user2"].Id,
            assignedToUserId: null,
            createdAtUtc: now.AddHours(-14),
            updatedAtUtc: now.AddHours(-14));

        AddAuditLog(fifthTicket, users["user2"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddHours(-14));

        var sixthTicket = CreateTicket(
            title: "Monthly reports export timing out",
            description: "Finance exports from the reporting portal time out for datasets above 25 MB.",
            priority: TicketPriority.Critical,
            status: TicketStatus.InProgress,
            createdByUserId: users["user3"].Id,
            assignedToUserId: users["agent2"].Id,
            createdAtUtc: now.AddDays(-1),
            updatedAtUtc: now.AddHours(-4));

        AddComment(sixthTicket, users["user3"].Id, "This is blocking our month-end close process.", now.AddDays(-1));
        AddComment(sixthTicket, users["agent2"].Id, "Query timeout increased for testing while I inspect the export job logs.", now.AddHours(-6));
        AddAuditLog(sixthTicket, users["user3"].Id, AuditActionType.TicketCreated, null, TicketStatus.Open.ToString(), now.AddDays(-1));
        AddAuditLog(sixthTicket, users["admin"].Id, AuditActionType.AgentAssigned, null, $"{users["agent2"].FullName} ({users["agent2"].Id})", now.AddHours(-20));
        AddAuditLog(sixthTicket, users["agent2"].Id, AuditActionType.StatusChanged, TicketStatus.Open.ToString(), TicketStatus.InProgress.ToString(), now.AddHours(-18));
        AddAuditLog(sixthTicket, users["agent2"].Id, AuditActionType.PriorityChanged, TicketPriority.High.ToString(), TicketPriority.Critical.ToString(), now.AddHours(-12));

        return [firstTicket, secondTicket, thirdTicket, fourthTicket, fifthTicket, sixthTicket];
    }

    private static Ticket CreateTicket(
        string title,
        string description,
        TicketPriority priority,
        TicketStatus status,
        string createdByUserId,
        string? assignedToUserId,
        DateTime createdAtUtc,
        DateTime? updatedAtUtc)
    {
        return new Ticket
        {
            Title = title,
            Description = description,
            Priority = priority,
            Status = status,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    private static void AddComment(Ticket ticket, string authorUserId, string content, DateTime createdAtUtc)
    {
        ticket.Comments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = authorUserId,
            Content = content,
            CreatedAtUtc = createdAtUtc
        });
    }

    private static void AddAuditLog(
        Ticket ticket,
        string changedByUserId,
        AuditActionType actionType,
        string? oldValue,
        string? newValue,
        DateTime changedAtUtc)
    {
        ticket.AuditLogs.Add(new TicketAuditLog
        {
            TicketId = ticket.Id,
            ChangedByUserId = changedByUserId,
            ActionType = actionType,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAtUtc = changedAtUtc
        });
    }

    private string GetPassword(string roleName)
    {
        return roleName switch
        {
            RoleNames.Admin => _seedOptions.AdminPassword ?? _seedOptions.DefaultPassword,
            RoleNames.Agent => _seedOptions.AgentPassword ?? _seedOptions.DefaultPassword,
            RoleNames.User => _seedOptions.UserPassword ?? _seedOptions.DefaultPassword,
            _ => _seedOptions.DefaultPassword
        };
    }
}
