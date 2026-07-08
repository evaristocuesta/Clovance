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
            userId: Guid.CreateVersion7(),
            email: "test@example.com");

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
}
