using System.Security.Cryptography;
using System.Text;

namespace Clovance.ApiService.Infrastructure.Auth.Token;

public class TokenService : ITokenService
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_'); // URL-safe
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
