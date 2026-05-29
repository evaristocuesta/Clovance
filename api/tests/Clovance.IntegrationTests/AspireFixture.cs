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

        // Initialize JWT token service with configuration from API project
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

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
