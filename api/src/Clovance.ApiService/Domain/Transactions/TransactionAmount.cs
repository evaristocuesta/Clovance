using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public sealed class TransactionAmount : ValueObject
{
    private TransactionAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static TransactionAmount Create(decimal value)
    {
        if (value == 0)
        {
            throw new ArgumentException("Transaction amount cannot be zero.", nameof(value));
        }

        return new TransactionAmount(decimal.Round(value, 2, MidpointRounding.ToEven));
    }

    public bool IsIncome => Value > 0;

    public bool IsExpense => Value < 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString("0.00");
    }
}
