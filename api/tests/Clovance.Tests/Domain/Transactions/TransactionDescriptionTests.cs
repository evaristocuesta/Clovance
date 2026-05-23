using Clovance.ApiService.Domain.Transactions;

namespace Clovance.Tests.Domain.Transactions;

public class TransactionDescriptionTests
{
    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        var action = () => TransactionDescription.Create(" ");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WithTooLongValue_ThrowsArgumentException()
    {
        var value = new string('a', 251);

        var action = () => TransactionDescription.Create(value);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_TrimsValue()
    {
        var description = TransactionDescription.Create("  Grocery  ");

        Assert.Equal("Grocery", description.Value);
    }
}
