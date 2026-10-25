using System.Net;
using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.ChangePassword;
using Clovance.ApiService.Features.Auth.Refresh;

namespace Clovance.IntegrationTests.Features.Auth;

public class ChangePasswordEndpointTests : IntegrationTestBase
{
    private const string RefreshCookieName = "clovance-refresh-token";

    public ChangePasswordEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ChangePassword_RevokesOtherSessionsAndKeepsCurrentSessionAlive()
    {
        // Arrange - two independent sessions for the same user (e.g. the owner and a stolen session)
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();
        var (currentAccessToken, currentRefreshToken) = await LoginUserAsync(email, password);
        var (_, otherSessionRefreshToken) = await LoginUserAsync(email, password);

        using var client = CreateClient(handleCookies: false);

        // Act
        using var changeRequest = new HttpRequestMessage(HttpMethod.Put, "/api/auth/users/password")
        {
            Content = JsonContent.Create(new ChangePasswordCommand(password, "NewPassword123!"))
        };
        changeRequest.Headers.Add("Authorization", $"Bearer {currentAccessToken}");
        changeRequest.Headers.Add("Cookie", $"{RefreshCookieName}={currentRefreshToken}");

        var changeResponse = await client.SendAsync(changeRequest, ct);

        // Assert
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        var newRefreshToken = ExtractRefreshCookie(changeResponse);
        Assert.NotEqual(currentRefreshToken, newRefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(client, otherSessionRefreshToken, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(client, currentRefreshToken, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await RefreshAsync(client, newRefreshToken, ct)).StatusCode);
    }

    private static async Task<HttpResponseMessage> RefreshAsync(
        HttpClient client,
        string refreshToken,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new RefreshCommand())
        };
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}");

        return await client.SendAsync(request, ct);
    }

    private static string ExtractRefreshCookie(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var values));

        var cookie = values!.First(v => v.StartsWith($"{RefreshCookieName}=", StringComparison.Ordinal));
        return cookie.Split(';')[0][(RefreshCookieName.Length + 1)..];
    }
}
