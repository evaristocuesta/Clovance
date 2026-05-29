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
            mustCompleteOnboarding: true);
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
}
