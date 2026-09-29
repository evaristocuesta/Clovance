using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts.DeleteAccount;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Accounts;

public class DeleteAccountCommandHandlerTests : AccountHandlerTestBase
{
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _handler = new DeleteAccountCommandHandler(Context, HttpContextAccessor);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountExistsAndUserIsAuthenticated_DeletesAccount()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Authenticate(userId);

        var result = await _handler.HandleAsync(new DeleteAccountCommand(account.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        var updated = await Context.Accounts
            .SingleAsync(a => a.Id == AccountId.Create(account.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(updated.IsDeleted);
        Assert.Equal(userId, updated.DeletedBy);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ReturnsAccountNotFoundError()
    {
        var userId = Guid.CreateVersion7();
        Authenticate(userId);

        var result = await _handler.HandleAsync(new DeleteAccountCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ReturnsUnauthorizedError()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Savings", AccountType.Savings, "USD", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await _handler.HandleAsync(new DeleteAccountCommand(account.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error?.Code);
    }

    }
