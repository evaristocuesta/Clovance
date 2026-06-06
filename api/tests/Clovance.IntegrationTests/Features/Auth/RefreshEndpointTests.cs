using System.Net.Http.Json;
using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Features.Auth.Logout;
using Clovance.ApiService.Features.Auth.Refresh;
using Microsoft.AspNetCore.Http;

namespace Clovance.IntegrationTests.Features.Auth;

public class RefreshEndpointTests : IntegrationTestBase
{
    public RefreshEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RefreshToken_WithoutCookie_ReturnsUnauthorized()
    {
        // Arrange
        var user = "test-nocookie@example.com";
        var password = "Password123!";
        await CreateTestUserAsync(user, password);

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand(Email: user, Password: password),
            TestContext.Current.CancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginResult);

        // Act - Try refresh WITHOUT cookie using a new client without cookies
        using var clientWithoutCookies = new HttpClient
        {
            BaseAddress = Client.BaseAddress
        };

        var response = await clientWithoutCookies.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshCommand(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var user = "test-notoken@example.com";
        var password = "Password123!";
        await CreateTestUserAsync(user, password);

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand(Email: user, Password: password),
            TestContext.Current.CancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginResult);

        // Extract refresh token from Set-Cookie header
        if (!loginResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            throw new InvalidOperationException("No Set-Cookie header found in login response");
        }

        var setCookieHeader = setCookieValues.FirstOrDefault();
        Assert.NotNull(setCookieHeader);

        // Act - Try refresh with cookie but WITHOUT Bearer token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new RefreshCommand())
        };

        request.Headers.Add("Cookie", setCookieHeader.Split(';')[0]); // Add only the cookie value part
        request.Headers.Add("Authorization", "Bearer ");

        using var clientWithoutCookies = new HttpClient
        {
            BaseAddress = Client.BaseAddress
        };

        var response = await clientWithoutCookies.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var user = "test-invalid-refresh@example.com";
        var password = "Password123!";
        await CreateTestUserAsync(user, password);

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand(Email: user, Password: password),
            TestContext.Current.CancellationToken);

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(loginResult);

        // Extract refresh token from Set-Cookie header
        if (!loginResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            throw new InvalidOperationException("No Set-Cookie header found in login response");
        }

        var setCookieHeader = setCookieValues.FirstOrDefault();
        Assert.NotNull(setCookieHeader);

        // Act - Try refresh with cookie but WITHOUT Bearer token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new RefreshCommand())
        };

        request.Headers.Add("Cookie", "refreshToken=no-valid-refresh-token");
        request.Headers.Add("Authorization", $"Bearer {loginResult.AccessToken}");

        using var clientWithoutCookies = new HttpClient
        {
            BaseAddress = Client.BaseAddress
        };

        var response = await clientWithoutCookies.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }



    [Fact]
    public async Task RefreshToken_ShouldReturnNewToken()
    {
        // Arrange
        var user = "test1@example.com";
        var password = "Password123!";
        await CreateTestUserAsync(user, password);
        var (token, refreshToken) = await LoginUserAsync(user, password);
        AuthenticateWithToken(token);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshCommand(),
            TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<RefreshResult>(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.NotNull(result.Token);
    }
}
