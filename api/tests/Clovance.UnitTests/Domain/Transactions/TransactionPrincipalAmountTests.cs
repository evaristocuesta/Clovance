using Clovance.ApiService.Domain.Transactions;

namespace Clovance.UnitTests.Domain.Transactions;

public class TransactionPrincipalAmountTests
{
    [Fact]
    public void Create_WhenValueIsNull_KeepsNull()
    {
        var principalAmount = TransactionPrincipalAmount.Create(null);

        Assert.Null(principalAmount.Value);
    }

    [Fact]
    public void Create_RoundsPrincipalAmountToTwoDecimals()
    {
        var principalAmount = TransactionPrincipalAmount.Create(123.456m);

        Assert.Equal(123.46m, principalAmount.Value);
    }
}
