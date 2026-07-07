using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public sealed class TransactionDate : ValueObject
{
    private TransactionDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static TransactionDate Create(DateOnly value)
    {
        return new TransactionDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString("O");
    }
}
