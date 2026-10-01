using Clovance.ApiService.Features.Transactions;

namespace Clovance.IntegrationTests.Features.Transactions;

public class GetTransactionByIdEndpointTests : TransactionIntegrationTestBase
{
    public GetTransactionByIdEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetTransactionByIdEndpoint_ReturnsTransaction()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var created = await CreateTransactionAsync(account.Id, 120m, description: "Salary");

        var response = await Client.GetAsync($"/api/transactions/{created.Id}", TestContext.Current.CancellationToken);
        var transaction = await ReadJsonAsync<TransactionDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(transaction);
        Assert.Equal(created.Id, transaction!.Id);
    }

    [Fact]
    public async Task GetTransactionByIdEndpoint_ReturnsNotFound_ForNonExistentTransaction()
    {
        AuthenticateAsRegularUser();

        var response = await Client.GetAsync($"/api/transactions/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionByIdEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.GetAsync($"/api/transactions/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
