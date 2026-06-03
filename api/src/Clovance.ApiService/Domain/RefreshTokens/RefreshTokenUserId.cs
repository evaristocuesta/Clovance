using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.RefreshTokens;

public class RefreshTokenUserId : ValueObject
{
    private RefreshTokenUserId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RefreshTokenUserId Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("User ID is required.", nameof(value));
        }

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            throw new ArgumentException("User ID is not valid.", nameof(value));
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
