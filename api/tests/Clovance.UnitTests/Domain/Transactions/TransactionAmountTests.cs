using Clovance.ApiService.Domain.Transactions;

namespace Clovance.Tests.Domain.Transactions;

public class TransactionAmountTests
{
    [Fact]
    public void Create_WithZero_ThrowsArgumentException()
    {
        var action = () => TransactionAmount.Create(0m);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_RoundsToTwoDecimals()
    {
        var amount = TransactionAmount.Create(12.345m);

        Assert.Equal(12.34m, amount.Value);
    }
}
