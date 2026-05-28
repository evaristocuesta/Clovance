using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;

namespace Clovance.Tests.Domain.Shared;

public class EntityIdTests
{
    [Fact]
    public void AccountId_CreateWithEmptyGuid_ThrowsArgumentException()
    {
        Action action = () => _ = AccountId.Create(Guid.Empty);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void TransactionId_CreateWithEmptyGuid_ThrowsArgumentException()
    {
        Action action = () => _ = TransactionId.Create(Guid.Empty);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void NewIds_AreNotDefault()
    {
        var accountId = AccountId.New();
        var transactionId = TransactionId.New();

        Assert.NotEqual(default, accountId);
        Assert.NotEqual(default, transactionId);
    }
}
