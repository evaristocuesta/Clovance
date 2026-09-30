using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Transactions.UpdateTransfer;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Transactions;

public class UpdateTransferCommandHandlerTests : TransactionHandlerTestBase
{
    private readonly UpdateTransferCommandHandler _handler;

    public UpdateTransferCommandHandlerTests()
    {
        _handler = new UpdateTransferCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenTransferExistsAndUserIsAuthenticated_UpdatesTransferPair()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(fromAccount.Id.Value, toAccount.Id.Value, 150m, "Move funds", new DateOnly(2025, 6, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new UpdateTransferCommand(
            transfer.From.Id.Value,
            new DateOnly(2025, 6, 5),
            "Updated transfer",
            300m,
            fromAccount.Id.Value,
            toAccount.Id.Value);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(-300m, result.Value.FromTransaction.Amount);
        Assert.Equal(300m, result.Value.ToTransaction.Amount);
        Assert.Equal("Updated transfer", result.Value.FromTransaction.Description);

        var updated = await Context.Transactions
            .OrderBy(t => t.Amount.Value)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Contains(updated, t => t.Id == transfer.From.Id && t.Amount.Value == -300m);
        Assert.Contains(updated, t => t.Id == transfer.To.Id && t.Amount.Value == 300m);
    }

    [Fact]
    public async Task HandleAsync_WhenTransferNotFound_ReturnsTransactionNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransferCommand(Guid.NewGuid(), new DateOnly(2025, 6, 5), "Updated transfer", 300m, Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Transactions.TransactionNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountIsInvalid_ReturnsInvalidAccountError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Mortgage", AccountType.Mortgage, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(fromAccount.Id.Value, toAccount.Id.Value, 150m, "Move funds", new DateOnly(2025, 6, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransferCommand(transfer.From.Id.Value, new DateOnly(2025, 6, 5), "Updated transfer", 300m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountTypeInvalid, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenFromAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddAsync(toAccount, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(Guid.NewGuid(), toAccount.Id.Value, 150m, "Move funds", new DateOnly(2025, 6, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransferCommand(transfer.From.Id.Value, new DateOnly(2025, 6, 5), "Updated transfer", 300m, Guid.NewGuid(), toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenToAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);

        await Context.Accounts.AddAsync(fromAccount, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(fromAccount.Id.Value, Guid.NewGuid(), 150m, "Move funds", new DateOnly(2025, 6, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(
            new UpdateTransferCommand(transfer.From.Id.Value, new DateOnly(2025, 6, 5), "Updated transfer", 300m, fromAccount.Id.Value, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var fromAccount = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var toAccount = CreateAccount("Savings", AccountType.Savings, "EUR", userId);

        await Context.Accounts.AddRangeAsync(new[] { fromAccount, toAccount }, TestContext.Current.CancellationToken);

        var transfer = CreateTransferPair(fromAccount.Id.Value, toAccount.Id.Value, 150m, "Move funds", new DateOnly(2025, 6, 1), userId);
        await Context.Transactions.AddRangeAsync(new[] { transfer.From, transfer.To }, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(
            new UpdateTransferCommand(transfer.From.Id.Value, new DateOnly(2025, 6, 5), "Updated transfer", 300m, fromAccount.Id.Value, toAccount.Id.Value),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }
}
