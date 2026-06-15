using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.GetUsers;

namespace Clovance.IntegrationTests.Features.Auth;

public class GetUsersEndpointTests : IntegrationTestBase
{
    public GetUsersEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUsersEndpoint_ReturnsUsers()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, AdminPassword);
        AuthenticateWithToken(token);

        // Act
        var response = await Client.GetAsync("/api/auth/users", TestContext.Current.CancellationToken);
        var users = await response.Content.ReadFromJsonAsync<GetUsersResult>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(users!.Users);
        Assert.NotEmpty(users.Users);
    }

    [Fact]
    public async Task GetUsersEndpoint_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        // Arrange - do not authenticate
        AuthenticateWithToken(string.Empty); // Clear any existing authentication

        // Act
        var response = await Client.GetAsync("/api/auth/users", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersEndpoint_ReturnsForbiddenForNonAdminUser()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com", "Password123!");
        var (token, refreshToken) = await LoginUserAsync(user.Email, "Password123!");
        AuthenticateWithToken(token);

        // Act
        var response = await Client.GetAsync("/api/auth/users", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
