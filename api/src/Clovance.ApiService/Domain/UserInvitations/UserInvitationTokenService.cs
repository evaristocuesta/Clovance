using System.Security.Cryptography;
using System.Text;

namespace Clovance.ApiService.Domain.UserInvitations;

public interface IUserInvitationTokenService
{
    string GenerateToken();

    string HashToken(string token);
}

public sealed class UserInvitationTokenService : IUserInvitationTokenService
{
    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
