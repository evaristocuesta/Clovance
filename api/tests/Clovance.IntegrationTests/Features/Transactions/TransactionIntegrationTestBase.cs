using System.Net.Http.Json;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Accounts;
using Clovance.ApiService.Features.Transactions;
using Clovance.ApiService.Features.Transactions.CreateLoanPayment;
using Clovance.ApiService.Features.Transactions.CreateTransaction;
using Clovance.ApiService.Features.Transactions.CreateTranfer;
using Clovance.IntegrationTests.Features.Accounts;

namespace Clovance.IntegrationTests.Features.Transactions;

public abstract class TransactionIntegrationTestBase : AccountIntegrationTestBase
{
    protected TransactionIntegrationTestBase(AspireFixture fixture) : base(fixture)
    {
    }

    protected async Task<TransactionDto> CreateTransactionAsync(
        Guid accountId,
        decimal amount = 100m,
        TransactionType type = TransactionType.Income,
        string description = "Transaction",
        DateOnly? date = null)
    {
        var command = new CreateTransactionCommand(
            Date: date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Description: description,
            Amount: amount,
            Type: type,
            AccountId: accountId);

        var response = await Client.PostAsJsonAsync("/api/transactions", command, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var transaction = await ReadJsonAsync<TransactionDto>(response.Content);
        Assert.NotNull(transaction);

        return transaction!;
    }

    protected async Task<CreateTransferResult> CreateTransferAsync(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount = 100m,
        string description = "Transfer",
        DateOnly? date = null)
    {
        var command = new CreateTransferCommand(
            Date: date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Description: description,
            Amount: amount,
            FromAccountId: fromAccountId,
            ToAccountId: toAccountId);

        var response = await Client.PostAsJsonAsync("/api/transactions/transfer", command, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await ReadJsonAsync<CreateTransferResult>(response.Content);
        Assert.NotNull(result);

        return result!;
    }

    protected async Task<CreateLoanPaymentResult> CreateLoanPaymentAsync(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount = 500m,
        decimal principalAmount = 200m,
        string description = "Loan payment",
        DateOnly? date = null)
    {
        var command = new CreateLoanPaymentCommand(
            Date: date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Description: description,
            Amount: amount,
            PrincipalAmount: principalAmount,
            FromAccountId: fromAccountId,
            ToAccountId: toAccountId);

        var response = await Client.PostAsJsonAsync("/api/transactions/loan-payment", command, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await ReadJsonAsync<CreateLoanPaymentResult>(response.Content);
        Assert.NotNull(result);

        return result!;
    }

    protected Task<AccountDto> CreateAssetAccountAsync(string? name = null)
    {
        return CreateAccountAsync(name ?? $"Asset-{Guid.CreateVersion7()}", AccountType.Checking, "EUR");
    }

    protected Task<AccountDto> CreateSecondAssetAccountAsync(string? name = null)
    {
        return CreateAccountAsync(name ?? $"Savings-{Guid.CreateVersion7()}", AccountType.Savings, "EUR");
    }

    protected Task<AccountDto> CreateLiabilityAccountAsync(string? name = null)
    {
        return CreateAccountAsync(name ?? $"Loan-{Guid.CreateVersion7()}", AccountType.Mortgage, "EUR");
    }
}
