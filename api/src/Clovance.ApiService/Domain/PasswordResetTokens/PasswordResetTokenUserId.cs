using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.PasswordResetTokens;

public class PasswordResetTokenUserId : ValueObject
{
    private PasswordResetTokenUserId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PasswordResetTokenUserId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(value));
        }

        return new PasswordResetTokenUserId(value);
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
