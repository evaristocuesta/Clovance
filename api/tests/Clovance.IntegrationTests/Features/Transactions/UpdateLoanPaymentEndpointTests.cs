using System.Net.Http.Json;
using Clovance.ApiService.Features.Transactions.UpdateLoanPayment;

namespace Clovance.IntegrationTests.Features.Transactions;

public class UpdateLoanPaymentEndpointTests : TransactionIntegrationTestBase
{
    public UpdateLoanPaymentEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateLoanPaymentEndpoint_ReturnsUpdatedLoanPaymentPair()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateLiabilityAccountAsync();
        var created = await CreateLoanPaymentAsync(fromAccount.Id, toAccount.Id, 400m, 150m, "Mortgage payment");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/loan-payment/{created.FromTransaction.Id}",
            new UpdateLoanPaymentRequest(new DateOnly(2025, 6, 10), "Updated mortgage payment", 600m, 250m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);
        var result = await ReadJsonAsync<UpdateLoanPaymentResult>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(-600m, result!.FromTransaction.Amount);
        Assert.Equal(-250m, result.FromTransaction.PrincipalAmount);
        Assert.Equal(250m, result.ToTransaction.Amount);
    }

    [Fact]
    public async Task UpdateLoanPaymentEndpoint_ReturnsNotFound_ForNonExistentLoanPayment()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateLiabilityAccountAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/loan-payment/{Guid.CreateVersion7()}",
            new UpdateLoanPaymentRequest(new DateOnly(2025, 6, 10), "Updated mortgage payment", 600m, 250m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLoanPaymentEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var fromAccount = await CreateAssetAccountAsync();
        var toAccount = await CreateLiabilityAccountAsync();
        var created = await CreateLoanPaymentAsync(fromAccount.Id, toAccount.Id, 400m, 150m, "Mortgage payment");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/loan-payment/{created.FromTransaction.Id}",
            new UpdateLoanPaymentRequest(new DateOnly(2025, 6, 10), "Updated mortgage payment", 600m, 700m, fromAccount.Id, toAccount.Id),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLoanPaymentEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/loan-payment/{Guid.CreateVersion7()}",
            new UpdateLoanPaymentRequest(new DateOnly(2025, 6, 10), "Updated mortgage payment", 600m, 250m, Guid.CreateVersion7(), Guid.CreateVersion7()),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
