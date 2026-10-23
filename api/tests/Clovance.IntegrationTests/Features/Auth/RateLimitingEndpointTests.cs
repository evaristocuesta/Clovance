using System.Net;
using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.IntegrationTests.Features.Auth;

public class RateLimitingEndpointTests : IntegrationTestBase
{
    public RateLimitingEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AuthEndpoints_ReturnTooManyRequests_WhenRateLimitExceeded()
    {
        var ct = TestContext.Current.CancellationToken;

        // Under the rate limit (Testing: 100 permits per window) the setup endpoint works.
        await CreateDefaultAdminUser();

        // Exhaust the shared auth rate limit with failed login attempts.
        HttpResponseMessage? rejected = null;
        for (var i = 0; i < 150 && rejected is null; i++)
        {
            var request = new LoginCommand(Email: AdminEmail, Password: "WrongPassword123!");
            var response = await Client.PostAsJsonAsync("/api/auth/login", request, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rejected = response;
            }
        }

        Assert.NotNull(rejected);

        var problem = await rejected!.Content.ReadFromJsonAsync<ProblemDetails>(ct);

        Assert.NotNull(problem);
        Assert.Equal(429, problem!.Status);
        Assert.Equal("common.too_many_requests", problem.Extensions["errorCode"]?.ToString());

        // Endpoints outside the auth rate limit policy are not affected.
        var dataResponse = await Client.GetAsync("/api/accounts", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, dataResponse.StatusCode);
    }
}
