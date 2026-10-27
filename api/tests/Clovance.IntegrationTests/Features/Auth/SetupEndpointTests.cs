using System.Net;
using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.RegisterAdmin;

namespace Clovance.IntegrationTests.Features.Auth;

public class SetupEndpointTests : IntegrationTestBase
{
    public SetupEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SetupEndpoint_WhenCalledConcurrently_CreatesOnlyOneAdmin()
    {
        // Arrange
        const int requestCount = 10;

        // Act
        var requests = Enumerable.Range(0, requestCount)
            .Select(index => Client.PostAsJsonAsync(
                "/api/auth/setup",
                new SetupCommand(
                    Email: $"admin-{index}@example.com",
                    Password: "Password123!",
                    FirstName: "Admin",
                    LastName: $"User{index}"),
                TestContext.Current.CancellationToken));

        var responses = await Task.WhenAll(requests);

        try
        {
            // Assert
            Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
            Assert.Equal(
                requestCount - 1,
                responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }
}
