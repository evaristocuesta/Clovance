using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Clovance.IntegrationTests;

/// <summary>
/// Shared Aspire fixture that starts the distributed application once for all tests in a collection.
/// </summary>
public class AspireFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

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
        var ct = TestContext.Current.CancellationToken;

        var jwtKeyFilePath = Path.Combine(
            Path.GetTempPath(), $"clovance-tests-{Guid.NewGuid()}", "jwt.key");
        Directory.CreateDirectory(Path.GetDirectoryName(jwtKeyFilePath)!);

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Clovance_AppHost>(
                args:
                [
                    "AddFrontend=false",
                    "--environment=Testing",
                    $"--Jwt:KeyFilePath={jwtKeyFilePath}"
                ],
                ct);

        Console.WriteLine($"[DEBUG] AppHost environment = {appHost.Environment.EnvironmentName}");

        using var cts = new CancellationTokenSource(DefaultTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

        _app = await appHost
            .BuildAsync(linkedCts.Token)
            .WaitAsync(DefaultTimeout, linkedCts.Token);

        await _app
            .StartAsync(linkedCts.Token)
            .WaitAsync(DefaultTimeout, linkedCts.Token);

        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync(
                resourceName: "clovance-apiservice",
                cancellationToken: linkedCts.Token)
            .WaitAsync(DefaultTimeout, linkedCts.Token);

        Client = _app.CreateHttpClient("clovance-apiservice");

        var jwtKey = await File.ReadAllTextAsync(jwtKeyFilePath, linkedCts.Token);

        var apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "Clovance.ApiService");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .Build();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        jwtOptions.Key = jwtKey;

        _jwtTokenService = new JwtTokenService(Options.Create(jwtOptions));
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
