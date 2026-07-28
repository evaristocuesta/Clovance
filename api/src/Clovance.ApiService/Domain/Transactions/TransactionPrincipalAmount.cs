using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Transactions;

public class TransactionPrincipalAmount : ValueObject
{
    private TransactionPrincipalAmount(decimal? value)
    {
        Value = value;
    }

    public decimal? Value { get; }

    public static TransactionPrincipalAmount Create(decimal? value)
    {
        return new TransactionPrincipalAmount(value is not null ? decimal.Round(value.Value, 2, MidpointRounding.ToEven) : null);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value?.ToString("0.00") ?? "null";
    }
}
