using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts.UpdateAccount;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Accounts;

public class UpdateAccountCommandHandlerTests : AccountHandlerTestBase
{
    private readonly UpdateAccountCommandHandler _handler;

    public UpdateAccountCommandHandlerTests()
    {
        _handler = new UpdateAccountCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountExistsAndUserIsAuthenticated_UpdatesAccount()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var command = new UpdateAccountCommand(account.Id.Value, "Savings", AccountType.Savings, "USD");
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Savings", result.Value.Account.Name);
        Assert.Equal(AccountType.Savings, result.Value.Account.Type);
        Assert.Equal("USD", result.Value.Account.Currency);

        var updated = await Context.Accounts
            .SingleAsync(a => a.Id == AccountId.Create(account.Id.Value), TestContext.Current.CancellationToken);

        Assert.Equal("Savings", updated.Name.Value);
        Assert.Equal(AccountType.Savings, updated.Type);
        Assert.Equal("USD", updated.Currency.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(new UpdateAccountCommand(Guid.NewGuid(), "Budget", AccountType.Checking, "EUR"), TestContext.Current.CancellationToken);

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

        var result = await _handler.HandleAsync(new UpdateAccountCommand(account.Id.Value, "Savings", AccountType.Savings, "USD"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }

    }
