using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts;
using Clovance.ApiService.Features.Accounts.UpdateAccount;

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
            new UpdateAccountRequest($"Updated-{Guid.CreateVersion7()}", AccountType.Savings, "USD"),
            TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(account);
        Assert.Equal(AccountType.Savings, account!.Type);
        Assert.Equal("USD", account.Currency);
        Assert.StartsWith("Updated-", account.Name);
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{Guid.CreateVersion7()}",
            new UpdateAccountRequest("Updated", AccountType.Savings, "USD"),
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
            new UpdateAccountRequest("", AccountType.Savings, "USD"),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccountEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsJsonAsync(
            $"/api/accounts/{Guid.CreateVersion7()}",
            new UpdateAccountRequest("Updated", AccountType.Savings, "USD"),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
