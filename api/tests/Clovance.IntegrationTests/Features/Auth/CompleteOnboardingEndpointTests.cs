using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.CompleteOnboarding;

namespace Clovance.IntegrationTests.Features.Auth;

public class CompleteOnboardingEndpointTests : IntegrationTestBase
{
    public CompleteOnboardingEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CompleteOnboardingEndpoint_ReturnsSuccess()
    {
        // Arrange - authenticate with user that has not completed onboarding
        AuthenticateAsAdminUser();
        var adminUser = await GetAdminUserAsync(Client);
        AuthenticateWithToken(GenerateTestToken(adminUser.Id, adminUser.Email, adminUser.MustCompleteOnboarding, adminUser.Roles));
        var user = await CreateTestUserAsync(Client, null, null, requiresOnboarding: true, roles: ["Admin"]);
        var token = await LoginUserAsync(client: Client, email: user.Email, password: user.Password);
        AuthenticateWithToken(token);

        // Act - call the complete onboarding endpoint
        var response = await Client.PutAsJsonAsync("/api/auth/complete-onboarding",
            new CompleteOnboardingCommand(user.Password, "NewPassword123!", user.Email),
            TestContext.Current.CancellationToken);

        // Assert - should return 204 No Content and user should be marked as having completed onboarding
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CompleteOnboardingEndpoint_ReturnsForbidden_ForNotAdminUser()
    {
        // Arrange - authenticate with user that has already completed onboarding
        AuthenticateAsAdminUser();
        var adminUser = await GetAdminUserAsync(Client);
        AuthenticateWithToken(GenerateTestToken(adminUser.Id, adminUser.Email, adminUser.MustCompleteOnboarding, adminUser.Roles));
        var user = await CreateTestUserAsync(Client, null, null, requiresOnboarding: true);
        var token = await LoginUserAsync(client: Client, email: user.Email, password: user.Password);
        AuthenticateWithToken(token);

        // Act - call the complete onboarding endpoint
        var response = await Client.PutAsJsonAsync("/api/auth/complete-onboarding",
            new CompleteOnboardingCommand(user.Password, "NewPassword123!", user.Email),
            TestContext.Current.CancellationToken);

        // Assert - should return 403 Forbidden
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CompleteOnboardingEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        // Arrange - authenticate with user that has not completed onboarding
        AuthenticateAsOnboardingUser();

        // Act - call the complete onboarding endpoint with invalid data
        var response = await Client.PutAsJsonAsync("/api/auth/complete-onboarding",
            new CompleteOnboardingCommand("", "", ""), // Invalid: empty values
            TestContext.Current.CancellationToken);

        // Assert - should return 400 Bad Request
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteOnboardingEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        // Arrange - do not authenticate

        // Act - call the complete onboarding endpoint
        var response = await Client.PutAsJsonAsync("/api/auth/complete-onboarding",
            new CompleteOnboardingCommand("SomePassword123!", "NewPassword123!", "test@example.com"),
            TestContext.Current.CancellationToken);

        // Assert - should return 401 Unauthorized
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

}
