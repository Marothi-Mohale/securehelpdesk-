using System.Data.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Domain.Enums;
using SecureHelpdesk.Infrastructure.Data;
using Xunit;

namespace SecureHelpdesk.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=integration-tests",
                ["Jwt:Issuer"] = "SecureHelpdesk.Tests",
                ["Jwt:Audience"] = "SecureHelpdesk.Tests.Client",
                ["Jwt:SecretKey"] = "ThisIsAnIntegrationTestJwtSecretKey123!",
                ["Jwt:ExpirationMinutes"] = "60",
                ["DemoData:SeedOnStartup"] = "false",
                ["DemoData:AllowInNonDevelopment"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbConnection>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
                options.UseSqlite(serviceProvider.GetRequiredService<DbConnection>()));

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "IntegrationPolicy";
                    options.DefaultChallengeScheme = "IntegrationPolicy";
                    options.DefaultScheme = "IntegrationPolicy";
                })
                .AddPolicyScheme("IntegrationPolicy", "JWT or test auth", options =>
                {
                    options.ForwardDefaultSelector = context =>
                        context.Request.Headers.ContainsKey("X-Test-User-Id")
                            ? TestAuthHandler.SchemeName
                            : JwtBearerDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await SeedAsync(scope.ServiceProvider);
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        Dispose();
    }

    private static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin-id", "admin@test.local", "System Admin", "Password123!", RoleNames.Admin);
        var agent = await EnsureUserAsync(userManager, "agent-id", "agent@test.local", "Alice Agent", "Password123!", RoleNames.Agent);
        var user = await EnsureUserAsync(userManager, "user-id", "user@test.local", "Uma User", "Password123!", RoleNames.User);

        if (!await dbContext.Tickets.AnyAsync())
        {
            var ticket = new Ticket
            {
                Title = "Seeded integration ticket",
                Description = "Seeded ticket used by HTTP integration tests.",
                Priority = TicketPriority.High,
                Status = TicketStatus.Open,
                CreatedByUserId = user.Id,
                AssignedToUserId = agent.Id,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
                UpdatedAtUtc = DateTime.UtcNow.AddHours(-1)
            };

            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync();
        }

        static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string id,
            string email,
            string fullName,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = id,
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to seed integration test user {email}.");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }
    }
}
