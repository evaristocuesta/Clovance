using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.Login;
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

        // Act - Try refresh WITHOUT cookie
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new RefreshCommand())
        };
        request.Headers.Authorization = new("Bearer", loginResult.AccessToken);

        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

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
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new RefreshCommand())
        };

        request.Headers.Add("Cookie", $"refreshToken={refreshToken}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.Content.ReadFromJsonAsync<RefreshResult>(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.NotNull(result.Token);
    }
}
