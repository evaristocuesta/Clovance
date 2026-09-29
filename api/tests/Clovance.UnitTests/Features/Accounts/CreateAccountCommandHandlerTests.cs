using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Accounts.CreateAccount;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Accounts;

public class CreateAccountCommandHandlerTests : AccountHandlerTestBase
{
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _handler = new CreateAccountCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAuthenticated_CreatesAccountAndOpeningTransaction()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var openingDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = new CreateAccountCommand(
            "Main account",
            AccountType.Checking,
            "USD",
            1250.50m,
            openingDate,
            "Opening Balance");

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Main account", result.Value.Account.Name);
        Assert.Equal(AccountType.Checking, result.Value.Account.Type);
        Assert.Equal("USD", result.Value.Account.Currency);

        var account = await Context.Accounts.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Main account", account.Name.Value);
        Assert.Equal(userId, account.CreatedBy);

        var transaction = await Context.Transactions.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(TransactionType.OpeningBalance, transaction.Type);
        Assert.Equal(1250.50m, transaction.Amount.Value);
        Assert.Equal("Opening Balance", transaction.Description.Value);
        Assert.Equal(account.Id.Value, transaction.AccountId.Value);
        Assert.Equal(openingDate, transaction.Date.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(new CreateAccountCommand(
            "Main account",
            AccountType.Checking,
            "USD",
            1000m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Opening Balance"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }

    }
