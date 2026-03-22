using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using SecureHelpdesk.Application.DTOs.Auth;
using SecureHelpdesk.Application.DTOs.Common;
using SecureHelpdesk.Application.DTOs.Tickets;
using Xunit;

namespace SecureHelpdesk.IntegrationTests;

public class AuthAndAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApiWebApplicationFactory _factory;

    public AuthAndAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_Returns_Jwt_And_User_Profile()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "admin@test.local",
            Password = "Password123!"
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));
        Assert.Contains("Admin", payload.User.Roles);
    }

    [Fact]
    public async Task Unauthenticated_Tickets_Request_Returns_401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal("unauthorized", payload!.ErrorCode);
    }

    [Fact]
    public async Task Regular_User_Cannot_Access_Admin_Agent_Lookup()
    {
        var client = CreateTestAuthenticatedClient("user-id", "user@test.local", "User");

        var response = await client.GetAsync("/api/users/agents");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Get_Agent_Lookup()
    {
        var client = CreateTestAuthenticatedClient("admin-id", "admin@test.local", "Admin");

        var response = await client.GetAsync("/api/users/agents");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserLookupDto>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!);
        Assert.Contains(payload!, user => user.Email == "agent@test.local");
    }

    [Fact]
    public async Task Authenticated_User_Can_Create_Ticket_Through_Http_Pipeline()
    {
        var client = CreateTestAuthenticatedClient("user-id", "user@test.local", "User");

        var response = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequestDto
        {
            Title = "Need access to CRM reports",
            Description = "The CRM reporting section returns an authorization error for my account.",
            Priority = Domain.Enums.TicketPriority.High
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<TicketResponseDto>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("Need access to CRM reports", payload!.Title);
        Assert.Equal(Domain.Enums.TicketStatus.Open, payload.Status);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();
        var authPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authPayload!.Token);
        return client;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    private HttpClient CreateTestAuthenticatedClient(string userId, string email, string role)
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-User-Id", userId);
        client.DefaultRequestHeaders.Add("X-Test-Email", email);
        client.DefaultRequestHeaders.Add("X-Test-Roles", role);
        return client;
    }
}
