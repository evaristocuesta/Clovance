using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts.GetAccounts;

namespace Clovance.UnitTests.Features.Accounts;

public class GetAccountsQueryHandlerTests : AccountHandlerTestBase
{
    private readonly GetAccountsQueryHandler _handler;

    public GetAccountsQueryHandlerTests()
    {
        _handler = new GetAccountsQueryHandler(Context);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountsExist_ReturnsAllAccounts()
    {
        var userId = Guid.CreateVersion7();
        var account1 = CreateAccount("Checking", AccountType.Checking, "EUR", userId);
        var account2 = CreateAccount("Savings", AccountType.Savings, "USD", userId);

        await Context.Accounts.AddRangeAsync(account1, account2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.HandleAsync(new GetAccountsQuery(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Accounts.Count());
        Assert.Contains(result.Value.Accounts, a => a.Name == "Checking" && a.Currency == "EUR");
        Assert.Contains(result.Value.Accounts, a => a.Name == "Savings" && a.Currency == "USD");
    }
}
