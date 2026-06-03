using System.Net.Mail;
using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.UserInvitations;

public sealed class UserInvitationEmail : ValueObject
{
    private UserInvitationEmail(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserInvitationEmail Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        var normalizedEmail = value
            .Trim()
            .ToLowerInvariant();

        if (normalizedEmail.Length > 320)
        {
            throw new ArgumentException("Email is too long.", nameof(value));
        }

        if (!MailAddress.TryCreate(normalizedEmail, out var email))
        {
            throw new ArgumentException("Email is not valid.", nameof(value));
        }

        return new UserInvitationEmail(email.Address);
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
