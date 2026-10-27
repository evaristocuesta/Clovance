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
        AuthenticateAsRegularUser();

        var response = await Client.GetAsync("/api/transactions?dateFrom=2025-01-01&pageSize=10", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionsEndpoint_ReturnsBadRequest_WhenDescriptionExceedsMaxLength()
    {
        AuthenticateAsRegularUser();

        var description = new string('a', 251);
        var response = await Client.GetAsync(
            $"/api/transactions?description={Uri.EscapeDataString(description)}",
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionsEndpoint_TreatsLikeWildcardsAsLiteralCharacters()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var marker = Guid.CreateVersion7().ToString("N");
        var percentDescription = $"Discount!-{marker}-10%";
        var percentLookalikeDescription = $"Discount-{marker}-10X";
        var underscoreDescription = $"Invoice-{marker}_2026";
        var underscoreLookalikeDescription = $"Invoice-{marker}A2026";

        await CreateTransactionAsync(account.Id, description: percentDescription);
        await CreateTransactionAsync(account.Id, description: percentLookalikeDescription);
        await CreateTransactionAsync(account.Id, description: underscoreDescription);
        await CreateTransactionAsync(account.Id, description: underscoreLookalikeDescription);

        var percentResponse = await Client.GetAsync(
            $"/api/transactions?description={Uri.EscapeDataString($"Discount!-{marker}-10%")}&pageSize=200",
            TestContext.Current.CancellationToken);
        var percentPage = await ReadJsonAsync<GetTransactionsPageResponse>(percentResponse.Content);

        var underscoreResponse = await Client.GetAsync(
            $"/api/transactions?description={Uri.EscapeDataString($"{marker}_2026")}&pageSize=200",
            TestContext.Current.CancellationToken);
        var underscorePage = await ReadJsonAsync<GetTransactionsPageResponse>(underscoreResponse.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, percentResponse.StatusCode);
        Assert.NotNull(percentPage);
        Assert.Single(percentPage!.Items, item => item.Description == percentDescription);
        Assert.DoesNotContain(percentPage.Items, item => item.Description == percentLookalikeDescription);

        Assert.Equal(System.Net.HttpStatusCode.OK, underscoreResponse.StatusCode);
        Assert.NotNull(underscorePage);
        Assert.Single(underscorePage!.Items, item => item.Description == underscoreDescription);
        Assert.DoesNotContain(underscorePage.Items, item => item.Description == underscoreLookalikeDescription);
    }

    [Fact]
    public async Task GetTransactionsEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync("/api/transactions", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
