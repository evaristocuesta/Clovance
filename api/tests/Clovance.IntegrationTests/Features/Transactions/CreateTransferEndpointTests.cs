using System.Net.Http.Json;
using Clovance.ApiService.Features.Transactions.CreateTranfer;

namespace Clovance.IntegrationTests.Features.Transactions;

public class CreateTransferEndpointTests : TransactionIntegrationTestBase
{
    public CreateTransferEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateTransferEndpoint_ReturnsTransferPair()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateSecondAssetAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/transfer",
            new CreateTransferCommand(new DateOnly(2025, 3, 1), "Move funds", 200m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<CreateTransferResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(-200m, result!.FromTransaction.Amount);
        Assert.Equal(200m, result.ToTransaction.Amount);
    }

    [Fact]
    public async Task CreateTransferEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/transfer",
            new CreateTransferCommand(new DateOnly(2025, 3, 1), "Move funds", 200m, account.Id, account.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransferEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/transfer",
            new CreateTransferCommand(new DateOnly(2025, 3, 1), "Move funds", 200m, Guid.CreateVersion7(), Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
