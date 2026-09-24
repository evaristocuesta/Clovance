using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Clovance.IntegrationTests;

/// <summary>
/// Shared API fixture backed by a PostgreSQL Testcontainer.
/// </summary>
public sealed class AspireFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:18.3-alpine")
        .WithDatabase("clovance-database")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly string _jwtKeyFilePath = Path.Combine(
        Path.GetTempPath(),
        $"clovance-tests-{Guid.NewGuid()}",
        "jwt.key");

    private WebApplicationFactory<Program> _factory = null!;
    private IJwtTokenService _jwtTokenService = null!;

    public HttpClient Client { get; private set; } = null!;
    public IJwtTokenService JwtTokenService => _jwtTokenService;

    public HttpClient CreateClient(bool handleCookies = true)
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = handleCookies
        });
    }

    /// <summary>
    /// Tracks whether the admin user has been created for THIS test host instance.
    /// </summary>
    public bool AdminUserCreated { get; set; }

    /// <summary>
    /// Lock for thread-safe admin setup for THIS test host instance.
    /// </summary>
    public SemaphoreSlim AdminLock { get; } = new(1, 1);

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        Directory.CreateDirectory(Path.GetDirectoryName(_jwtKeyFilePath)!);

        using var cts = new CancellationTokenSource(DefaultTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

        await _postgresContainer
            .StartAsync(linkedCts.Token)
            .WaitAsync(DefaultTimeout, linkedCts.Token);

        _factory = new TestApiFactory(_postgresContainer.GetConnectionString(), _jwtKeyFilePath);
        Client = _factory.CreateClient();
        _jwtTokenService = _factory.Services.GetRequiredService<IJwtTokenService>();

        var response = await Client.GetAsync("/health", linkedCts.Token);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();

        var jwtDirectory = Path.GetDirectoryName(_jwtKeyFilePath);
        if (jwtDirectory is not null && Directory.Exists(jwtDirectory))
        {
            Directory.Delete(jwtDirectory, recursive: true);
        }
    }

    private sealed class TestApiFactory(string connectionString, string jwtKeyFilePath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:clovance-database", connectionString);
            builder.UseSetting("Jwt:KeyFilePath", jwtKeyFilePath);

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:clovance-database"] = connectionString,
                    ["Jwt:KeyFilePath"] = jwtKeyFilePath
                });
            });
        }
    }
}
