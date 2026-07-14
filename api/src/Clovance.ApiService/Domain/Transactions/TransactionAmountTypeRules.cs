namespace Clovance.ApiService.Domain.Transactions;

public class TransactionAmountTypeRules
{
    public static bool EnsureAmountMatchesType(decimal currentAmount, decimal newAmount, TransactionType type)
    {
        return type switch
        {
            TransactionType.Income => newAmount > 0,
            TransactionType.Expense => newAmount < 0,
            TransactionType.Transfer => newAmount != 0 && Math.Sign(currentAmount) == Math.Sign(newAmount),
            _ => false
        };
    }
}
