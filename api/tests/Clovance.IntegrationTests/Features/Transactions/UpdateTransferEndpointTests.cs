using System.Net.Http.Json;
using Clovance.ApiService.Features.Transactions.CreateTranfer;
using Clovance.ApiService.Features.Transactions.UpdateTransfer;

namespace Clovance.IntegrationTests.Features.Transactions;

public class UpdateTransferEndpointTests : TransactionIntegrationTestBase
{
    public UpdateTransferEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateTransferEndpoint_ReturnsUpdatedTransferPair()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateSecondAssetAccountAsync();
        var replacementToAccount = await CreateAssetAccountAsync();
        var created = await CreateTransferAsync(fromAccount.Id, toAccount.Id, 150m, "Move funds");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/transfer/{created.FromTransaction.Id}",
            new UpdateTransferRequest(new DateOnly(2025, 4, 5), "Updated transfer", 300m, fromAccount.Id, replacementToAccount.Id),
            TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<UpdateTransferResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(-300m, result!.FromTransaction.Amount);
        Assert.Equal(300m, result.ToTransaction.Amount);
        Assert.Equal(replacementToAccount.Id, result.ToTransaction.AccountId);
    }

    [Fact]
    public async Task UpdateTransferEndpoint_ReturnsNotFound_ForNonExistentTransfer()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateSecondAssetAccountAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/transfer/{Guid.CreateVersion7()}",
            new UpdateTransferRequest(new DateOnly(2025, 4, 5), "Updated transfer", 300m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransferEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var otherAccount = await CreateSecondAssetAccountAsync();
        var created = await CreateTransferAsync(account.Id, otherAccount.Id, 150m, "Move funds");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/transfer/{created.FromTransaction.Id}",
            new UpdateTransferRequest(new DateOnly(2025, 4, 5), "Updated transfer", 300m, account.Id, account.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransferEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/transfer/{Guid.CreateVersion7()}",
            new UpdateTransferRequest(new DateOnly(2025, 4, 5), "Updated transfer", 300m, Guid.CreateVersion7(), Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
