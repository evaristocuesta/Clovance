using System.Net;
using System.Net.Http.Json;
using Clovance.ApiService.Features.Auth.Login;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.IntegrationTests.Features.Auth;

public class LockoutEndpointTests : IntegrationTestBase
{
    private const string WrongPassword = "WrongPassword123!";

    public LockoutEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LoginEndpoint_LocksAccountAfterMaxFailedAttempts()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();

        // The first four failed attempts are reported as generic invalid credentials.
        for (var i = 0; i < 4; i++)
        {
            var failed = await PostLoginAsync(email, WrongPassword, ct);

            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
            Assert.Equal("auth.invalid_credentials", await GetErrorCodeAsync(failed, ct));
        }

        // The fifth failed attempt locks the account (5 attempts, 15 minutes).
        var locked = await PostLoginAsync(email, WrongPassword, ct);

        Assert.Equal((HttpStatusCode)423, locked.StatusCode);
        Assert.Equal("auth.account_locked", await GetErrorCodeAsync(locked, ct));

        // The correct password does not bypass the lockout.
        var correctWhileLocked = await PostLoginAsync(email, password, ct);

        Assert.Equal((HttpStatusCode)423, correctWhileLocked.StatusCode);
        Assert.Equal("auth.account_locked", await GetErrorCodeAsync(correctWhileLocked, ct));
    }

    [Fact]
    public async Task LoginEndpoint_ResetsFailedAccessCountOnSuccessfulLogin()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();

        // Reach one attempt below the lockout limit.
        for (var i = 0; i < 4; i++)
        {
            var failed = await PostLoginAsync(email, WrongPassword, ct);

            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        // A successful login resets the failed access count.
        var success = await PostLoginAsync(email, password, ct);

        Assert.Equal(HttpStatusCode.OK, success.StatusCode);

        // Four more failures after the reset must not lock the account.
        for (var i = 0; i < 4; i++)
        {
            var failed = await PostLoginAsync(email, WrongPassword, ct);

            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        var successAgain = await PostLoginAsync(email, password, ct);

        Assert.Equal(HttpStatusCode.OK, successAgain.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_ReturnsSameError_ForUnknownEmailAndPasswordMismatch()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, email, password) = await CreateTestUserAsync();

        var unknownEmail = await PostLoginAsync("unknown-user@example.com", password, ct);
        var wrongPassword = await PostLoginAsync(email, WrongPassword, ct);

        // Anti-enumeration: both cases expose the same generic error.
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var unknownEmailCode = await GetErrorCodeAsync(unknownEmail, ct);
        var wrongPasswordCode = await GetErrorCodeAsync(wrongPassword, ct);

        Assert.Equal("auth.invalid_credentials", unknownEmailCode);
        Assert.Equal(unknownEmailCode, wrongPasswordCode);
    }

    private async Task<HttpResponseMessage> PostLoginAsync(
        string email,
        string password,
        CancellationToken ct)
    {
        var request = new LoginCommand(Email: email, Password: password);
        return await Client.PostAsJsonAsync("/api/auth/login", request, ct);
    }

    private static async Task<string?> GetErrorCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        return problem?.Extensions["errorCode"]?.ToString();
    }
}
