using System.Net.Http.Json;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions;
using Clovance.ApiService.Features.Transactions.UpdateTransaction;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.IntegrationTests.Features.Transactions;

public class UpdateTransactionEndpointTests : TransactionIntegrationTestBase
{
    public UpdateTransactionEndpointTests(AspireFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateTransactionEndpoint_ReturnsUpdatedTransaction()
    {
        AuthenticateAsRegularUser();
        var firstAccount = await CreateAssetAccountAsync();
        var secondAccount = await CreateSecondAssetAccountAsync();
        var created = await CreateTransactionAsync(firstAccount.Id, 120m, description: "Salary");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/{created.Id}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), "Updated salary", 300m, TransactionType.Income, secondAccount.Id, created.Version),
            TestContext.Current.CancellationToken);
        var transaction = await ReadJsonAsync<TransactionDto>(response.Content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(transaction);
        Assert.Equal("Updated salary", transaction!.Description);
        Assert.Equal(300m, transaction.Amount);
        Assert.Equal(secondAccount.Id, transaction.AccountId);
    }

    [Fact]
    public async Task UpdateTransactionEndpoint_ReturnsConflict_ForStaleVersion()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var created = await CreateTransactionAsync(account.Id, 120m, description: "Salary");

        // The version the client read before editing. The first update saves and bumps xmin,
        // so reusing it models a second tab saving on top of data it no longer reflects.
        var staleVersion = created.Version;

        var firstUpdate = await Client.PutAsJsonAsync(
            $"/api/transactions/{created.Id}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), "First update", 300m, TransactionType.Income, account.Id, staleVersion),
            TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, firstUpdate.StatusCode);

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/{created.Id}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), "Second update", 400m, TransactionType.Income, account.Id, staleVersion),
            TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("common.stale_data", problem?.Extensions["errorCode"]?.ToString());
    }

    [Fact]
    public async Task UpdateTransactionEndpoint_ReturnsNotFound_ForNonExistentTransaction()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/{Guid.CreateVersion7()}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), "Updated salary", 300m, TransactionType.Income, account.Id, 0),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransactionEndpoint_ReturnsBadRequest_ForInvalidRequest()
    {
        AuthenticateAsRegularUser();
        var account = await CreateAssetAccountAsync();
        var created = await CreateTransactionAsync(account.Id, 120m, description: "Salary");

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/{created.Id}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), string.Empty, 300m, TransactionType.Income, account.Id, created.Version),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransactionEndpoint_ReturnsUnauthorized_ForUnauthenticatedUser()
    {
        AuthenticateWithToken(string.Empty);

        var response = await Client.PutAsJsonAsync(
            $"/api/transactions/{Guid.CreateVersion7()}",
            new UpdateTransactionRequest(new DateOnly(2025, 2, 5), "Updated salary", 300m, TransactionType.Income, Guid.CreateVersion7(), 0),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
