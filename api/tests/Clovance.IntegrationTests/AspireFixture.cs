using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Clovance.ApiService.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;

namespace Clovance.IntegrationTests;

/// <summary>
/// Shared Aspire fixture that starts the distributed application once for all tests in a collection.
/// </summary>
public class AspireFixture : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private IJwtTokenService _jwtTokenService = null!;

    public HttpClient Client { get; private set; } = null!;
    public IJwtTokenService JwtTokenService => _jwtTokenService;

    /// <summary>
    /// Tracks whether the admin user has been created for THIS Aspire instance.
    /// </summary>
    public bool AdminUserCreated { get; set; } = false;

    /// <summary>
    /// Lock for thread-safe admin setup for THIS Aspire instance.
    /// </summary>
    public SemaphoreSlim AdminLock { get; } = new(1, 1);

    public async ValueTask InitializeAsync()
    {
        // Set environment before creating the app host so it's picked up during build
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Create and start Aspire application
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Clovance_AppHost>();

        _app = await appHost.BuildAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await _app.StartAsync(cts.Token);

        // Create HTTP client for the API service
        Client = _app.CreateHttpClient("clovance-apiservice");

        // Initialize JWT token service with configuration from API project
        var apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "Clovance.ApiService");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .Build();

        _jwtTokenService = new JwtTokenService(configuration);

        // Wait for the service to be fully ready
        await Task.Delay(3000);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        // Clean up environment variables
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
}
