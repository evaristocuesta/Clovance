using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts.GetAccountById;
using Clovance.ApiService.Shared;

namespace Clovance.UnitTests.Features.Accounts;

public class GetAccountByIdQueryHandlerTests : AccountHandlerTestBase
{
    private readonly GetAccountByIdQueryHandler _handler;

    public GetAccountByIdQueryHandlerTests()
    {
        _handler = new GetAccountByIdQueryHandler(Context);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountExists_ReturnsAccount()
    {
        var userId = Guid.CreateVersion7();
        var account = CreateAccount("Primary", AccountType.Checking, "EUR", userId);
        await Context.Accounts.AddAsync(account, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(new GetAccountByIdQuery(account.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Account);
        Assert.Equal(account.Id.Value, result.Value.Account!.Id);
        Assert.Equal("Primary", result.Value.Account.Name);
        Assert.Equal("EUR", result.Value.Account.Currency);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountDoesNotExist_ReturnsAccountNotFoundError()
    {
        var result = await _handler.HandleAsync(new GetAccountByIdQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Accounts.AccountNotFound, result.Error?.Code);
    }
}
