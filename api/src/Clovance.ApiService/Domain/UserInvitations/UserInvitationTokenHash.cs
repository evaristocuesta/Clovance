using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.UserInvitations;

public sealed class UserInvitationTokenHash : ValueObject
{
    private UserInvitationTokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserInvitationTokenHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Token is required.", nameof(value));
        }

        var normalizedToken = value
            .Trim();

        if (normalizedToken.Length != 64)
        {
            throw new ArgumentException("Token is invalid.", nameof(value));
        }

        return new UserInvitationTokenHash(normalizedToken);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
