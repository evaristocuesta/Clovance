using System.Security.Cryptography;
using System.Text;
using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.PasswordResetTokens;

public sealed class PasswordResetTokenHash : ValueObject
{
    private PasswordResetTokenHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PasswordResetTokenHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Password reset token hash is required.", nameof(value));
        }

        if (value.Length != 64) // SHA-256 hex string
        {
            throw new ArgumentException("Password reset token hash has an invalid length.", nameof(value));
        }

        return new PasswordResetTokenHash(value);
    }

    public static PasswordResetTokenHash FromPlainTextToken(string plainTextToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextToken));
        var hex = Convert.ToHexString(bytes);
        return new PasswordResetTokenHash(hex);
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
