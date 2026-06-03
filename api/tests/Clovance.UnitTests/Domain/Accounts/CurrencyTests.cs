using Clovance.ApiService.Domain.Accounts;

namespace Clovance.UnitTests.Domain.Accounts;

public class CurrencyTests
{
    [Fact]
    public void Create_WithInvalidLength_ThrowsArgumentException()
    {
        var action = () => Currency.Create("EU");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_NormalizesToUppercase()
    {
        var currency = Currency.Create("eur");

        Assert.Equal("EUR", currency.Code);
    }
}
