using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts;
using Clovance.ApiService.Features.Accounts.UpdateAccount;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.IntegrationTests.Features.Accounts;

public class UpdateAccountEndpointTests : AccountIntegrationTestBase
{
    public UpdateAccountEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsUpdatedAccount()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{created.Id}",
            new UpdateAccountRequest($"Updated-{Guid.CreateVersion7()}", AccountType.Savings, "USD", created.Version),
            TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(account);
        Assert.Equal(AccountType.Savings, account!.Type);
        Assert.Equal("USD", account.Currency);
        Assert.StartsWith("Updated-", account.Name);
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsConflict_ForStaleVersion()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();

        // The version the client read before editing. The first update saves and bumps xmin,
        // so reusing it models a second tab saving on top of data it no longer reflects.
        var staleVersion = created.Version;

        var firstUpdate = await Client.PutAsJsonAsync(
            $"/api/accounts/{created.Id}",
            new UpdateAccountRequest("First update", AccountType.Savings, "USD", staleVersion),
            TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, firstUpdate.StatusCode);

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{created.Id}",
            new UpdateAccountRequest("Second update", AccountType.Savings, "USD", staleVersion),
            TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("common.stale_data", problem?.Extensions["errorCode"]?.ToString());
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{Guid.CreateVersion7()}",
            new UpdateAccountRequest("Updated", AccountType.Savings, "USD", 0),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var created = await CreateAccountAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{created.Id}",
            new UpdateAccountRequest("", AccountType.Savings, "USD", created.Version),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{Guid.CreateVersion7()}",
            new UpdateAccountRequest("Updated", AccountType.Savings, "USD", 0),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
