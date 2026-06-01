using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Clovance.IntegrationTests;

public class DiagnosticTests
{
    [Fact]
    public async Task CanCreateAndStartAspireApp_WithTestingEnvironment()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        try
        {
            Console.WriteLine("Step 1: Creating DistributedApplicationTestingBuilder...");
            var appHost = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.Clovance_AppHost>(ct);

            Console.WriteLine("Step 2: Building app...");
            await using var app = await appHost.BuildAsync(ct);

            Console.WriteLine("Step 3: Starting app (timeout: 5 minutes)...");
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

            await app.StartAsync(linkedCts.Token);

            Console.WriteLine("Step 4: App started successfully!");

            Console.WriteLine("Step 5: Creating HTTP client...");
            var client = app.CreateHttpClient("apiservice");

            Console.WriteLine("Step 6: Waiting for API to respond...");
            var maxAttempts = 60;
            var attempt = 0;
            var success = false;

            while (attempt < maxAttempts && !success)
            {
                attempt++;
                try
                {
                    var response = await client.GetAsync("/health", ct);
                    Console.WriteLine($"Attempt {attempt}: Status = {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                        Console.WriteLine("API is healthy!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt}: Exception = {ex.Message}");
                }

                if (!success)
                {
                    await Task.Delay(1000, ct);
                }
            }

            Assert.True(success, "API did not respond successfully within timeout");
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }
}
