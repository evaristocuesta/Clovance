using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.Logout;

namespace Clovance.IntegrationTests.Features.Auth;

public class LogoutEndpointTests : IntegrationTestBase
{
    public LogoutEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LogoutEndpoint_ReturnsNoContent()
    {
        // Arrange
        await EnsureAdminReadyAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, NewAdminPassword);
        AuthenticateWithToken(token);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutCommand(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task LogoutEndpoint_ReturnsUnauthorizedForInvalidUser()
    {
        // Arrange
        AuthenticateWithToken(string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutCommand(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
