using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Clovance.ApiService.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

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
        var jwtKeyFilePath = Path.Combine(
            Path.GetTempPath(), $"clovance-tests-{Guid.NewGuid()}", "jwt.key");
        Directory.CreateDirectory(Path.GetDirectoryName(jwtKeyFilePath)!);

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Clovance_AppHost>(
                args:
                [
                    "--environment=Testing",
                    $"--Jwt:KeyFilePath={jwtKeyFilePath}"
                ]);

        _app = await appHost.BuildAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await _app.StartAsync(cts.Token);

        Client = _app.CreateHttpClient("clovance-apiservice");

        var jwtKey = await File.ReadAllTextAsync(jwtKeyFilePath, cts.Token);

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
