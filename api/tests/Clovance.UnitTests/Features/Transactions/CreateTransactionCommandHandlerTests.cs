using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions.CreateTransaction;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class CreateTransactionCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAuthenticatedAndAccountExists_CreatesTransaction()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new CreateTransactionCommand(
            new DateOnly(2025, 1, 15),
            "Salary",
            1250m,
            TransactionType.Income,
            account.Id.Value);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1250m, result.Value.Transaction.Amount);
        Assert.Equal(TransactionType.Income, result.Value.Transaction.Type);
        Assert.Equal("Salary", result.Value.Transaction.Description);

        var transaction = await Context.Transactions
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1250m, transaction.Amount.Value);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(account.Id.Value, transaction.AccountId.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new CreateTransactionCommand(new DateOnly(2025, 1, 15), "Salary", 1250m, TransactionType.Income, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new CreateTransactionCommand(new DateOnly(2025, 1, 15), "Salary", 1250m, TransactionType.Income, account.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
