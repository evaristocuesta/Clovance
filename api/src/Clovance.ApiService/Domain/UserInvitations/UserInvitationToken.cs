using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.UserInvitations;

public sealed class UserInvitationToken : ValueObject
{
    private UserInvitationToken(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserInvitationToken Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Token is required.", nameof(value));
        }

        var normalizedToken = value
            .Trim();

        if (normalizedToken.Length > 200)
        {
            throw new ArgumentException("Token is too long.", nameof(value));
        }

        return new UserInvitationToken(normalizedToken);
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
