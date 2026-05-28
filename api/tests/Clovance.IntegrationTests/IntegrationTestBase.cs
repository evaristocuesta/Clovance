using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Clovance.ApiService.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;

namespace Clovance.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides Aspire infrastructure.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private IJwtTokenService _jwtTokenService = null!;

    protected HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Create and start Aspire application
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Clovance_AppHost>();

        _app = await appHost.BuildAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await _app.StartAsync(cts.Token);

        // Create HTTP client for the API service
        Client = _app.CreateHttpClient("apiservice");

        // Load configuration from the API project's appsettings files
        // This ensures tests use the exact same JWT configuration as the Development environment
        // Current directory during test execution is: tests/Clovance.IntegrationTests/bin/Debug/net10.0
        var apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "Clovance.ApiService");

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        _jwtTokenService = new JwtTokenService(configuration);

        // Wait for the service to be fully ready
        await Task.Delay(3000);
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
        var (token, _) = _jwtTokenService.GenerateToken(
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

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
