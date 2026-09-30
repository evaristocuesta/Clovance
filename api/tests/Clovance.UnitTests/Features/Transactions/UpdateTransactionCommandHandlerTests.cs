using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Transactions;
using Clovance.ApiService.Features.Transactions.UpdateTransaction;
using Clovance.ApiService.Shared;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class UpdateTransactionCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly UpdateTransactionCommandHandler _handler;

    public UpdateTransactionCommandHandlerTests()
    {
        _handler = new UpdateTransactionCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionExistsAndUserIsAuthenticated_UpdatesTransaction()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);

        var transaction = Transaction.Create(100m, TransactionType.Income, "Initial salary", account.Id.Value, new DateOnly(2025, 5, 1), userId);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new UpdateTransactionCommand(
            transaction.ToDto(account.Name.Value, account.Currency.Code) with
            {
                Date = new DateOnly(2025, 5, 10),
                Description = "Updated salary",
                Amount = 250m,
                Type = TransactionType.Income,
                AccountId = account.Id.Value,
                AccountName = account.Name.Value,
                Currency = account.Currency.Code
            });

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(250m, result.Value.Transaction.Amount);
        Assert.Equal("Updated salary", result.Value.Transaction.Description);

        var updated = await Context.Transactions
            .SingleAsync(t => t.Id == transaction.Id, TestContext.Current.CancellationToken);

        Assert.Equal(250m, updated.Amount.Value);
        Assert.Equal("Updated salary", updated.Description.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ReturnsTransactionNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransactionCommand(new TransactionDto(Guid.NewGuid(), new DateOnly(2025, 6, 1), "Salary", 100m, null, TransactionType.Income, Guid.NewGuid(), "Checking", "EUR", null)),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Transactions.TransactionNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);

        var transaction = Transaction.Create(100m, TransactionType.Income, "Initial salary", account.Id.Value, new DateOnly(2025, 5, 1), userId);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransactionCommand(transaction.ToDto(account.Name.Value, account.Currency.Code) with { AccountId = Guid.NewGuid() }),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountCannotAcceptThisTransactionType_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);

        var transaction = Transaction.Create(100m, TransactionType.Income, "Initial salary", account.Id.Value, new DateOnly(2025, 5, 1), userId);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransactionCommand(transaction.ToDto(account.Name.Value, account.Currency.Code) with { Type = TransactionType.Income }),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountTypeInvalid, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);

        var transaction = Transaction.Create(100m, TransactionType.Income, "Initial salary", account.Id.Value, new DateOnly(2025, 5, 1), userId);
        await Context.Transactions.AddAsync(transaction, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new Microsoft.AspNetCore.Http.DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new UpdateTransactionCommand(transaction.ToDto(account.Name.Value, account.Currency.Code) with { Amount = 200m, Description = "Updated salary" }),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
