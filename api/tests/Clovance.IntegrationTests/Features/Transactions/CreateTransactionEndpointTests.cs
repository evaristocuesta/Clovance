using System.Net.Http.Json;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions;
using Clovance.ApiService.Features.Transactions.CreateTransaction;

namespace Clovance.IntegrationTests.Features.Transactions;

public class CreateTransactionEndpointTests : TransactionIntegrationTestBase
{
    public CreateTransactionEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateTransactionEndpoint_ReturnsCreatedTransaction()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions",
            new CreateTransactionCommand(new DateOnly(2025, 1, 10), "Salary", 250m, TransactionType.Income, account.Id),
            TestContext.Current.CancellationToken);
        var transaction = await ReadJsonAsync<TransactionDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(transaction);
        Assert.Equal("Salary", transaction!.Description);
        Assert.Equal(250m, transaction.Amount);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(account.Id, transaction.AccountId);
    }

    [Fact]
    public async Task CreateTransactionEndpoint_ReturnsNotFound_ForNonExistentAccount()
    {
        AuthenticateAsRegularUser();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions",
            new CreateTransactionCommand(new DateOnly(2025, 1, 10), "Salary", 250m, TransactionType.Income, Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransactionEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions",
            new CreateTransactionCommand(new DateOnly(2025, 1, 10), string.Empty, 250m, TransactionType.Income, account.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransactionEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        var account = Guid.CreateVersion7();
        AuthenticateWithToken(string.Empty);

        var response = await Client.PostAsJsonAsync(
            "/api/transactions",
            new CreateTransactionCommand(new DateOnly(2025, 1, 10), "Salary", 250m, TransactionType.Income, account),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
