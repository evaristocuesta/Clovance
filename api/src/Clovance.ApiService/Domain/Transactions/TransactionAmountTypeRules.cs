namespace Clovance.ApiService.Domain.Transactions;

public class TransactionAmountTypeRules
{
    public static bool EnsureAmountMatchesType(decimal amount, TransactionType type)
    {
        return type switch
        {
            TransactionType.Income => amount > 0,
            TransactionType.Expense => amount < 0,
            TransactionType.Transfer => amount != 0,
            _ => false
        };
    }
}
