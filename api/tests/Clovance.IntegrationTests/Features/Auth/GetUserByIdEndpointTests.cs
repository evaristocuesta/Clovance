namespace Clovance.IntegrationTests.Features.Auth;

public class GetUserByIdEndpointTests : IntegrationTestBase
{
    public GetUserByIdEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUserByIdEndpoint_ReturnsUser()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var (token, refreshToken) = await LoginUserAsync(AdminEmail, AdminPassword);
        AuthenticateWithToken(token);

        // Act
        var response = await Client.GetAsync($"/api/auth/users/{user.UserID}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByIdEndpoint_ReturnsNotFoundForNonExistentUser()
    {
        // Arrange
        AuthenticateAsAdminUser();
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var response = await Client.GetAsync($"/api/auth/users/{nonExistentUserId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByIdEndpoint_ReturnsUnauthorizedForUnauthenticatedUser()
    {
        // Arrange
        AuthenticateWithToken(string.Empty); // Clear any existing authentication

        // Act
        var response = await Client.GetAsync($"/api/auth/users/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByIdEndpoint_ReturnsForbiddenForNonAdminUser()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com", "Password123!");
        var (token, refreshToken) = await LoginUserAsync(user.Email, "Password123!");
        AuthenticateWithToken(token);

        // Act
        var response = await Client.GetAsync($"/api/auth/users/{user.UserID}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
