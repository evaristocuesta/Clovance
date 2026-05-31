using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Features.Auth.GetUserById;
using Clovance.ApiService.Features.Auth.GetUsers;
using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;
using Clovance.ApiService.Infrastructure.Authentication;

namespace Clovance.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides shared access to Aspire infrastructure.
/// Uses IClassFixture to share the Aspire app instance across all tests in the class.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    protected HttpClient Client => _fixture.Client;
    private IJwtTokenService JwtTokenService => _fixture.JwtTokenService;

    protected IntegrationTestBase(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Generates a test JWT token with the specified claims.
    /// Note: This uses the same JWT configuration loaded from the appsettings files of the API project.
    /// </summary>
    protected string GenerateTestToken(
        string userId = "test-user-id",
        string email = "test@example.com",
        bool mustCompleteOnboarding = false,
        IEnumerable<string>? roles = null)
    {
        var (token, _) = JwtTokenService.GenerateToken(
            userId,
            email,
            roles ?? [],
            mustCompleteOnboarding);

        return token;
    }

    /// <summary>
    /// Authenticates the HTTP client with a test token.
    /// </summary>
    protected void AuthenticateWithToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    /// <summary>
    /// Authenticates with a token for a user who must complete onboarding.
    /// </summary>
    protected void AuthenticateAsOnboardingUser()
    {
        var token = GenerateTestToken(
            userId: "onboarding-user-id",
            email: "onboarding@example.com",
            mustCompleteOnboarding: true,
            roles: ["Admin"]);
        AuthenticateWithToken(token);
    }

    /// <summary>
    /// Authenticates with a token for a regular user (onboarding complete).
    /// </summary>
    protected void AuthenticateAsRegularUser()
    {
        var token = GenerateTestToken(
            userId: "regular-user-id",
            email: "regular@example.com",
            mustCompleteOnboarding: false);
        AuthenticateWithToken(token);
    }

    /// <summary>
    /// Authenticates with a token for a admin user (onboarding complete).
    /// </summary>
    protected void AuthenticateAsAdminUser()
    {
        var token = GenerateTestToken(
            userId: "admin-user-id",
            email: "admin@example.com",
            mustCompleteOnboarding: false,
            roles: ["Admin"]);
        AuthenticateWithToken(token);
    }

    /// <summary>
    /// Creates a test user via the registration API.
    /// Returns the user's email and password for later authentication.
    /// </summary>
    public async Task<(string UserID, string Email, string Password)> CreateTestUserAsync(
        HttpClient client,
        string? email = null,
        string? password = null,
        bool requiresOnboarding = false,
        IEnumerable<string>? roles = null)
    {
        email ??= $"test-{Guid.NewGuid()}@example.com";
        password ??= "TestPassword123!";

        var request = new CreateInvitationCommand(Email: email, IsAdmin: roles?.Contains("Admin") ?? false);

        var response = await client.PostAsJsonAsync("/api/auth/invitations", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateInvitationResult>();

        var res = await client.PostAsJsonAsync("/api/auth/register-with-invitation", new
        {
            Email = email,
            Password = password,
            result!.Token,
        });

        var registrationResult = await res.Content.ReadFromJsonAsync<RegisterWithInvitationResult>();

        res.EnsureSuccessStatusCode();

        return (registrationResult!.UserId, email, password);
    }

    /// <summary>
    /// Gets all users via the API.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/users");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetUsersResult>();
        return result!.Users;
    }

    public async Task<UserDto> GetAdminUserAsync(HttpClient client)
    {
        var users = await GetAllUsersAsync(client);
        var adminUser = users.FirstOrDefault(u => u.Roles.Contains("Admin"));

        if (adminUser is null)
        {
            throw new InvalidOperationException("Admin user not found.");
        }

        return adminUser;
    }

    public async Task<UserDto> GetUserById(HttpClient client, string id)
    {
        var response = await client.GetAsync($"/api/users/{id}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetUserByIdResult>();
        return result!.User;
    }

    /// <summary>
    /// Authenticates a user and returns the JWT token.
    /// </summary>
    public async Task<string> LoginUserAsync(
        HttpClient client,
        string email,
        string password)
    {
        var request = new { email, password };
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.AccessToken;
    }
}
