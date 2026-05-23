using Clovance.ApiService.Domain.Accounts;

namespace Clovance.Tests.Domain.Shared;

public class ValueObjectTests
{
    [Fact]
    public void ValueObjects_WithSameValues_AreEqual()
    {
        var left = Currency.Create("eur");
        var right = Currency.Create("EUR");

        Assert.True(left == right);
        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void ValueObjects_WithDifferentValues_AreNotEqual()
    {
        var left = AccountName.Create("Main");
        var right = AccountName.Create("Savings");

        Assert.True(left != right);
        Assert.NotEqual(left, right);
    }
}
