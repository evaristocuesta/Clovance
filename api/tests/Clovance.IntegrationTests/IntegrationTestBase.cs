using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Features.Auth.GetUserById;
using Clovance.ApiService.Features.Auth.GetUsers;
using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Features.Auth.RegisterAdmin;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;
using Clovance.ApiService.Infrastructure.Authentication;

namespace Clovance.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides shared access to Aspire infrastructure.
/// Uses IClassFixture to share the Aspire app instance across all tests in the class.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<AspireFixture>
{
    protected const string AdminEmail = "admin@clovance.local";
    protected const string AdminPassword = "NewAdmin.Password.123";
    protected const string AdminFirstName = "FirsName";
    protected const string AdminLastName = "LastName";

    private readonly AspireFixture _fixture;

    protected HttpClient Client => _fixture.Client;
    private IJwtTokenService _jwtTokenService => _fixture.JwtTokenService;

    protected IntegrationTestBase(
        AspireFixture fixture)
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
        IEnumerable<string>? roles = null)
    {
        var (token, _) = _jwtTokenService.GenerateToken(
            userId,
            email,
            roles ?? []);

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
    /// Authenticates with a token for a regular user.
    /// </summary>
    protected void AuthenticateAsRegularUser()
    {
        var token = GenerateTestToken(
            userId: "regular-user-id",
            email: "regular@example.com");
        AuthenticateWithToken(token);
    }

    /// <summary>
    /// Authenticates with a token for a admin user.
    /// </summary>
    protected void AuthenticateAsAdminUser()
    {
        var token = GenerateTestToken(
            userId: "admin-user-id",
            email: "admin@example.com",
            roles: ["Admin"]);
        AuthenticateWithToken(token);
    }

    public async Task CreateDefaultAdminUser()
    {
        var setupRequest = new SetupCommand(Email: AdminEmail, Password: AdminPassword, FirstName: AdminFirstName, LastName: AdminLastName);
        var response = await Client.PostAsJsonAsync("/api/auth/setup", setupRequest);
        response.EnsureSuccessStatusCode();
        _fixture.AdminUserCreated = true;
    }

    /// <summary>
    /// Creates a test user via the registration API.
    /// Returns the user's ID, email, and password for later authentication.
    /// </summary>
    public async Task<(string UserID, string Email, string Password)> CreateTestUserAsync(
        string? email = null,
        string? password = null,
        IEnumerable<string>? roles = null)
    {
        email ??= $"test-{Guid.NewGuid()}@example.com";
        password ??= "TestPassword123!";

        // Determine if the user should be an admin based on roles
        bool isAdmin = roles?.Contains("Admin") ?? false;

        // Get an authenticated admin token
        var (adminToken, adminRefreshToken) = await EnsureAdminReadyAsync();
        AuthenticateWithToken(adminToken);

        // Create an invitation
        var invitationRequest = new CreateInvitationCommand(Email: email, IsAdmin: isAdmin);
        var invitationResponse = await Client.PostAsJsonAsync("/api/auth/invitations", invitationRequest);
        invitationResponse.EnsureSuccessStatusCode();
        var invitation = await invitationResponse.Content.ReadFromJsonAsync<CreateInvitationResult>();

        // Clear authentication to register as a new user
        Client.DefaultRequestHeaders.Authorization = null;

        // Register with the invitation
        var registerRequest = new RegisterWithInvitationCommand(Email: email, Password: password, Token: invitation!.Token, FirstName: "FirstName", LastName: "LastName");
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterWithInvitationResult>();

        return (registerResult!.UserId, email, password);
    }

    /// <summary>
    /// Ensures the admin user is created and returns a valid token.
    /// Thread-safe and caches the admin user creation state per Aspire instance.
    /// </summary>
    protected async Task<(string Token, string RefreshToken)> EnsureAdminReadyAsync()
    {
        await _fixture.AdminLock.WaitAsync();

        try
        {
            if (!_fixture.AdminUserCreated)
            {
                await CreateDefaultAdminUser();
            }

            // Admin already set up in this Aspire instance, just login with new password
            return await LoginUserAsync(AdminEmail, AdminPassword);
        }
        finally
        {
            _fixture.AdminLock.Release();
        }
    }

    /// <summary>
    /// Gets all users via the API.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var response = await Client.GetAsync("/api/auth/users");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetUsersResult>();
        return result!.Users;
    }

    public async Task<UserDto> GetAdminUserAsync()
    {
        var users = await GetAllUsersAsync();
        var adminUser = users.FirstOrDefault(u => u.Roles.Contains("Admin"));

        if (adminUser is null)
        {
            throw new InvalidOperationException("Admin user not found.");
        }

        return adminUser;
    }

    public async Task<UserDto> GetUserById(string id)
    {
        var response = await Client.GetAsync($"/api/users/{id}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetUserByIdResult>();
        return result!.User;
    }

    /// <summary>
    /// Authenticates a user and returns the JWT token and refresh token.
    /// </summary>
    public async Task<(string Token, string RefreshToken)> LoginUserAsync(
        string email,
        string password)
    {
        var request = new LoginCommand(Email: email, Password: password);
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Extract refresh token from Set-Cookie header
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            throw new InvalidOperationException("No Set-Cookie header found in login response");
        }

        var setCookieHeader = setCookieValues.FirstOrDefault();
        Assert.NotNull(setCookieHeader);

        var refreshToken = ExtractCookieValue(setCookieHeader, "refreshToken");
        Assert.NotNull(refreshToken);

        return (result!.AccessToken, refreshToken);
    }

    private static string? ExtractCookieValue(string setCookieHeader, string cookieName)
    {
        var cookies = setCookieHeader.Split(';');
        var cookiePair = cookies[0].Split('=');

        if (cookiePair.Length == 2 && cookiePair[0].Trim() == cookieName)
        {
            return cookiePair[1].Trim();
        }

        return null;
    }
}
