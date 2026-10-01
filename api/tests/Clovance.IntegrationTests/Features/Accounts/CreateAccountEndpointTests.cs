using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts;
using Clovance.ApiService.Features.Accounts.CreateAccount;

namespace Clovance.IntegrationTests.Features.Accounts;

public class CreateAccountEndpointTests : AccountIntegrationTestBase
{
    public CreateAccountEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateAccountEndpoint_ReturnsCreatedAccount()
    {
        AuthenticateAsRegularUser();

        var command = new CreateAccountCommand(
            Name: $"Checking-{Guid.CreateVersion7()}",
            Type: AccountType.Checking,
            Currency: "EUR",
            OpeningBalance: 100m,
            OpeningDate: new DateOnly(2025, 1, 10),
            OpeningDescription: "Opening balance");

        var response = await Client.PostAsJsonAsync("/api/accounts", command, TestContext.Current.CancellationToken);
        var account = await ReadJsonAsync<AccountDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(account);
        Assert.Equal(command.Name, account!.Name);
        Assert.Equal(command.Type, account.Type);
        Assert.Equal(command.Currency, account.Currency);
        Assert.False(account.IsDeleted);
    }

    [Fact]
    public async Task CreateAccountEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();

        var response = await Client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountCommand("", AccountType.Checking, "EUR", 100m, new DateOnly(2025, 1, 10), "Opening balance"),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccountEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountCommand("Checking", AccountType.Checking, "EUR", 100m, new DateOnly(2025, 1, 10), "Opening balance"),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
