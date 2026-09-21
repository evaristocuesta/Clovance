using System.Net;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clovance.IntegrationTests.Infrastructure;

public class AspireTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Clovance_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("clovance-frontend");

        var maxAttempts = 60;
        var attempt = 0;
        var frontendHealthy = false;

        while (attempt < maxAttempts && !frontendHealthy)
        {
            attempt++;
            try
            {
                var healthResponse = await httpClient.GetAsync("/healthz.txt", cancellationToken);

                if (healthResponse.IsSuccessStatusCode)
                {
                    frontendHealthy = true;
                }
            }
            catch
            {
                // Ignore and retry
            }

            if (!frontendHealthy)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        Assert.True(frontendHealthy, "Frontend did not respond successfully on /healthz.txt within timeout.");

        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
