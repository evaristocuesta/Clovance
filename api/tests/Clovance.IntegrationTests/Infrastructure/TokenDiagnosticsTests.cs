using System.Net.Http.Headers;

namespace Clovance.IntegrationTests.Infrastructure;

/// <summary>
/// Diagnostic tests to verify JWT token generation and validation.
/// </summary>
public class TokenDiagnosticsTests : IntegrationTestBase
{
    public TokenDiagnosticsTests(AspireFixture fixture) : base(fixture)
    {
    }
    [Fact]
    public async Task GeneratedToken_CanBeValidated()
    {
        // Arrange - generate a test token
        var token = GenerateTestToken(
            userId: "test-user",
            email: "test@example.com",
            mustCompleteOnboarding: false);

        // Print the token to console for debugging
        Console.WriteLine($"Generated token (false): {token}");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - call a protected endpoint that requires authentication
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert - should not be 401 (token should be valid)
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Console.WriteLine($"Response status (false): {response.StatusCode}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        // We expect 204 NoContent for successful logout
    }

    [Fact]
    public async Task OnboardingToken_ReturnsForbidden()
    {
        // Arrange - generate a test token with onboarding required
        var token = GenerateTestToken(
            userId: "onboarding-test-user",
            email: "onboarding@example.com",
            mustCompleteOnboarding: true);

        // Print the token to console for debugging
        Console.WriteLine($"Generated token: {token}");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - call a protected endpoint
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert - should get 403 Forbidden (not 401 Unauthorized)
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Console.WriteLine($"Response status: {response.StatusCode}");
        Console.WriteLine($"Response content: {content}");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}
