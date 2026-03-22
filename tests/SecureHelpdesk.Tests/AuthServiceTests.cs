using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Constants;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Infrastructure.Identity;
using Xunit;

namespace SecureHelpdesk.Tests;

public class AuthServiceTests
{
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();

    [Fact]
    public async Task RegisterAsync_Creates_User_Assigns_Default_Role_And_Returns_Token()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = CreateUserManager(userStore.Object);
        var signInManager = CreateSignInManager(userManager.Object);

        ApplicationUser? createdUser = null;

        userManager.Setup(manager => manager.FindByEmailAsync("new.user@test.local"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = "user-123";
                createdUser = user;
            })
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync([RoleNames.User]);

        _tokenService.Setup(service => service.CreateTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(("generated-token", new DateTime(2026, 03, 22, 15, 0, 0, DateTimeKind.Utc)));

        var service = new AuthService(userManager.Object, signInManager.Object, _tokenService.Object, _logger.Object);

        var result = await service.RegisterAsync(
            new RegisterRequestDto
            {
                FullName = "  New User  ",
                Email = "  New.User@Test.Local ",
                Password = "Password123!"
            },
            CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.Equal("new.user@test.local", createdUser!.Email);
        Assert.Equal("new.user@test.local", createdUser.UserName);
        Assert.Equal("New User", createdUser.FullName);
        Assert.Equal("generated-token", result.Token);
        Assert.Contains(RoleNames.User, result.User.Roles);

        userManager.Verify(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User), Times.Once);
        _tokenService.Verify(service => service.CreateTokenAsync(
            It.Is<ApplicationUser>(user => user.Email == "new.user@test.local"),
            It.Is<IReadOnlyCollection<string>>(roles => roles.Contains(RoleNames.User))),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Throws_Conflict_When_Email_Already_Exists()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = CreateUserManager(userStore.Object);
        var signInManager = CreateSignInManager(userManager.Object);

        userManager.Setup(manager => manager.FindByEmailAsync("existing@test.local"))
            .ReturnsAsync(new ApplicationUser { Id = "existing-1", Email = "existing@test.local" });

        var service = new AuthService(userManager.Object, signInManager.Object, _tokenService.Object, _logger.Object);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.RegisterAsync(
            new RegisterRequestDto
            {
                FullName = "Existing User",
                Email = "existing@test.local",
                Password = "Password123!"
            },
            CancellationToken.None));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
        userManager.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Returns_Token_When_Credentials_Are_Valid()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = CreateUserManager(userStore.Object);
        var signInManager = CreateSignInManager(userManager.Object);
        var user = new ApplicationUser
        {
            Id = "admin-1",
            Email = "admin@test.local",
            UserName = "admin@test.local",
            FullName = "System Admin"
        };

        userManager.Setup(manager => manager.FindByEmailAsync("admin@test.local"))
            .ReturnsAsync(user);
        userManager.Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync([RoleNames.Admin]);
        signInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        _tokenService.Setup(service => service.CreateTokenAsync(user, It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(("jwt-token", new DateTime(2026, 03, 22, 16, 0, 0, DateTimeKind.Utc)));

        var service = new AuthService(userManager.Object, signInManager.Object, _tokenService.Object, _logger.Object);

        var result = await service.LoginAsync(
            new LoginRequestDto
            {
                Email = " Admin@Test.Local ",
                Password = "Password123!"
            },
            CancellationToken.None);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("System Admin", result.User.FullName);
        Assert.Contains(RoleNames.Admin, result.User.Roles);
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_For_Unknown_User()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = CreateUserManager(userStore.Object);
        var signInManager = CreateSignInManager(userManager.Object);

        userManager.Setup(manager => manager.FindByEmailAsync("missing@test.local"))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new AuthService(userManager.Object, signInManager.Object, _tokenService.Object, _logger.Object);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.LoginAsync(
            new LoginRequestDto
            {
                Email = "missing@test.local",
                Password = "Password123!"
            },
            CancellationToken.None));

        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
        signInManager.Verify(manager => manager.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), true), Times.Never);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_Returns_Profile_With_Roles()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = CreateUserManager(userStore.Object);
        var signInManager = CreateSignInManager(userManager.Object);
        var user = new ApplicationUser
        {
            Id = "agent-1",
            Email = "agent@test.local",
            UserName = "agent@test.local",
            FullName = "Alice Agent"
        };

        userManager.Setup(manager => manager.FindByIdAsync("agent-1"))
            .ReturnsAsync(user);
        userManager.Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync([RoleNames.Agent]);

        var service = new AuthService(userManager.Object, signInManager.Object, _tokenService.Object, _logger.Object);

        var result = await service.GetCurrentUserProfileAsync("agent-1", CancellationToken.None);

        Assert.Equal("Alice Agent", result.FullName);
        Assert.Contains(RoleNames.Agent, result.Roles);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager(IUserStore<ApplicationUser> store)
    {
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(accessor => accessor.Value).Returns(new IdentityOptions());

        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManager(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(accessor => accessor.Value).Returns(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();

        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            confirmation.Object);
    }
}
