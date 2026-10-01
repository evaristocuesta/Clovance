using Clovance.ApiService.Features.Transactions;

namespace Clovance.IntegrationTests.Features.Transactions;

public class DeleteTransactionEndpointTests : TransactionIntegrationTestBase
{
    public DeleteTransactionEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DeleteTransactionEndpoint_ReturnsNoContent_AndDeletesTransaction()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var created = await CreateTransactionAsync(account.Id, 120m, description: "Salary");

        var deleteResponse = await Client.DeleteAsync($"/api/transactions/{created.Id}", TestContext.Current.CancellationToken);
        var getResponse = await Client.GetAsync($"/api/transactions/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTransactionEndpoint_ReturnsNotFound_ForNonExistentTransaction()
    {
        AuthenticateAsRegularUser();

        var response = await Client.DeleteAsync($"/api/transactions/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransactionEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.DeleteAsync($"/api/transactions/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
