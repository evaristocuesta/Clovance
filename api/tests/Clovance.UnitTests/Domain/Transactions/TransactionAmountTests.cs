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

    [Fact]
    public void Negate_ReturnsNegatedValue()
    {
        var amount = TransactionAmount.Create(12.34m);
        var negatedAmount = amount.Negate();
        Assert.Equal(-12.34m, negatedAmount.Value);
    }
}
