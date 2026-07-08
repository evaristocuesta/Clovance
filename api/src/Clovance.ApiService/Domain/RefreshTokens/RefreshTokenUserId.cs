using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public class RefreshTokenUserId : ValueObject
{
    private RefreshTokenUserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RefreshTokenUserId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(value));
        }

        return new RefreshTokenUserId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
