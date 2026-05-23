using Clovance.ApiService.Domain.Accounts;

namespace Clovance.Tests.Domain.Accounts;

public class AccountNameTests
{
    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        var action = () => AccountName.Create("  ");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_TrimsValue()
    {
        var accountName = AccountName.Create("  Main Account  ");

        Assert.Equal("Main Account", accountName.Value);
    }
}
