using System.Net.Http.Json;
using Clovance.ApiService.Features.Transactions.CreateLoanPayment;

namespace Clovance.IntegrationTests.Features.Transactions;

public class CreateLoanPaymentEndpointTests : TransactionIntegrationTestBase
{
    public CreateLoanPaymentEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateLoanPaymentEndpoint_ReturnsLoanPaymentPair()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateLiabilityAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/loan-payment",
            new CreateLoanPaymentCommand(new DateOnly(2025, 5, 1), "Mortgage payment", 500m, 200m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<CreateLoanPaymentResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(-500m, result!.FromTransaction.Amount);
        Assert.Equal(-200m, result.FromTransaction.PrincipalAmount);
        Assert.Equal(200m, result.ToTransaction.Amount);
    }

    [Fact]
    public async Task CreateLoanPaymentEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateLiabilityAccountAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/loan-payment",
            new CreateLoanPaymentCommand(new DateOnly(2025, 5, 1), "Mortgage payment", 500m, 700m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoanPaymentEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PostAsJsonAsync(
            "/api/transactions/loan-payment",
            new CreateLoanPaymentCommand(new DateOnly(2025, 5, 1), "Mortgage payment", 500m, 200m, Guid.CreateVersion7(), Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
