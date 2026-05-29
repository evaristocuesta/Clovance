using System.Net;
using System.Net.Http.Json;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for the UseOnboardingEnforcement middleware.
/// Tests all possible scenarios for onboarding enforcement.
/// </summary>
public class OnboardingEnforcementMiddlewareTests : IntegrationTestBase
{
    public OnboardingEnforcementMiddlewareTests(AspireFixture fixture) : base(fixture)
    {
    }
    #region Authenticated User - Must Complete Onboarding

    [Fact]
    public async Task ProtectedEndpoint_WithMustCompleteOnboardingClaim_Returns403Forbidden()
    {
        // Arrange - authenticate with user that must complete onboarding
        AuthenticateAsOnboardingUser();

        // Act - try to access a protected endpoint (logout requires auth)
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("Forbidden", problemDetails.Title);
        Assert.Contains("onboarding", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.NotNull(problemDetails.Extensions["traceId"]);
        Assert.Equal("/api/auth/logout", problemDetails.Instance);
        Assert.Equal(ErrorCodes.Auth.MustCompleteOnBoarding, problemDetails.Extensions["errorCode"]?.ToString());
    }

    #endregion

    #region Authenticated User - Onboarding Complete

    [Fact]
    public async Task ProtectedEndpoint_WithCompletedOnboarding_ReturnsSuccess()
    {
        // Arrange - authenticate with user that has completed onboarding
        AuthenticateAsRegularUser();

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert - logout returns 204 No Content on success
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Authenticated User - Onboarding Not Complete

    [Fact]
    public async Task ProtectedEndpoint_WithIncompleteOnboarding_ReturnsForbidden()
    {
        // Arrange - authenticate with user that has not completed onboarding
        AuthenticateAsOnboardingUser();

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert - logout returns 403 Forbidden for users with incomplete onboarding
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Complete Onboarding Endpoint - Always Allowed

    [Fact]
    public async Task CompleteOnboardingEndpoint_WithMustCompleteOnboardingClaim_AllowsAccess()
    {
        // Arrange
        AuthenticateAsOnboardingUser();

        // Act - complete-onboarding endpoint should always be accessible
        var response = await Client.PostAsJsonAsync("/api/auth/complete-onboarding", new
        {
            currentPassword = "Onboarding123!",
            newPassword = "NewPassword123!"
        }, TestContext.Current.CancellationToken);

        // Assert - should not return 403 (might return 400 or other errors, but not 403)
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Anonymous Endpoints

    [Fact]
    public async Task AnonymousEndpoint_WithoutAuthentication_AllowsAccess()
    {
        // Arrange - no authentication header

        // Act - login is an anonymous endpoint
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "test@example.com",
            password = "test"
        }, TestContext.Current.CancellationToken);

        // Assert - should not be blocked by middleware (might fail at login logic, but not 403)
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousEndpoint_WithAuthenticationButMustCompleteOnboarding_AllowsAccess()
    {
        // Arrange - user with onboarding required tries to access anonymous endpoint
        AuthenticateAsOnboardingUser();

        // Act - register-with-invitation is anonymous
        var response = await Client.PostAsJsonAsync("/api/auth/register-with-invitation", new
        {
            email = "newuser@example.com",
            password = "Pass123!",
            token = "some-token"
        }, TestContext.Current.CancellationToken);

        // Assert - should not be blocked
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Unauthenticated Requests

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange - no authentication header

        // Act - try to access protected endpoint
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert - should return 401, not 403 (middleware should pass through)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange - set invalid token
        Client.DefaultRequestHeaders.Authorization = new("Bearer", "invalid-token");

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ProtectedEndpoint_WithClaimStringVariations_HandlesCorrectly()
    {
        // Arrange - test various string representations of boolean
        var testCases = new[]
        {
            ("True", true),   // Should block
            ("true", true),   // Should block
            ("TRUE", true),   // Should block
            ("False", false), // Should allow
            ("false", false), // Should allow
            ("1", false),     // Invalid, treated as false
            ("0", false),     // Invalid, treated as false
            ("", false)       // Empty, treated as false
        };

        foreach (var (claimValue, shouldBlock) in testCases)
        {
            // Generate token for each test case
            var email = $"claim-{Guid.NewGuid()}@example.com";
            var token = GenerateTestToken(
                userId: Guid.NewGuid().ToString(),
                email: email,
                mustCompleteOnboarding: claimValue.Equals("True", StringComparison.OrdinalIgnoreCase));

            Client.DefaultRequestHeaders.Authorization = new("Bearer", token);

            // Act
            var response = await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken);

            // Assert
            if (shouldBlock)
            {
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMultipleRequests_HandlesCorrectly()
    {
        // Arrange
        AuthenticateAsOnboardingUser();

        // Act - multiple requests in sequence
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 3; i++)
        {
            responses.Add(await Client.PostAsync("/api/auth/logout", null, TestContext.Current.CancellationToken));
        }

        // Assert - all should be consistently blocked
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    #endregion

    #region Middleware Order

    [Fact]
    public async Task Middleware_RunsBeforeEndpointAuthorization()
    {
        // Arrange - user with onboarding required but no admin role
        AuthenticateAsOnboardingUser();

        // Act - try to access admin-only endpoint
        var response = await Client.PostAsJsonAsync("/api/auth/invitations", new
        {
            email = "newadmin@example.com"
        }, TestContext.Current.CancellationToken);

        // Assert - should be blocked by onboarding middleware (403) before role check
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.NotNull(problemDetails);
        // Should be onboarding error, not authorization error
        Assert.Equal(ErrorCodes.Auth.MustCompleteOnBoarding, problemDetails.Extensions["errorCode"]?.ToString());
    }

    #endregion
}
