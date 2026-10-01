using System.Net.Http.Json;
using Clovance.ApiService.Features.Transactions.GetTransactions;

namespace Clovance.IntegrationTests.Features.Transactions;

public class GetTransactionsEndpointTests : TransactionIntegrationTestBase
{
    public GetTransactionsEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetTransactionsEndpoint_ReturnsFilteredTransactions()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var otherAccount = await CreateSecondAssetAccountAsync();
        await CreateTransactionAsync(account.Id, 150m, description: "Salary", date: new DateOnly(2025, 1, 10));
        await CreateTransactionAsync(account.Id, -40m, type: Clovance.ApiService.Domain.Transactions.TransactionType.Expense, description: "Groceries", date: new DateOnly(2025, 1, 12));
        await CreateTransactionAsync(otherAccount.Id, 300m, description: "Bonus", date: new DateOnly(2025, 1, 15));

        var response = await Client.GetAsync(
            $"/api/transactions?dateFrom=2025-01-01&dateTo=2025-01-31&accountId={account.Id}&pageSize=10",
            TestContext.Current.CancellationToken);
        var page = await ReadJsonAsync<GetTransactionsPageResponse>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(page);
        Assert.Equal(2, page!.Items.Count);
        Assert.All(page.Items, item => Assert.Equal(account.Id, item.AccountId));
    }

    [Fact]
    public async Task GetTransactionsEndpoint_ReturnsBadRequest_ForInvalidQuery()
    {
        var response = await Client.GetAsync("/api/transactions?dateFrom=2025-01-01&pageSize=10", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
