using Clovance.ApiService.Domain.Transactions;

namespace Clovance.UnitTests.Domain.Transactions;

public class TransactionAmountTests
{
    [Fact]
    public void Create_RoundsToTwoDecimals()
    {
        var amount = TransactionAmount.Create(12.345m);

        Assert.Equal(12.34m, amount.Value);
    }
}
