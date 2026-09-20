using System.Security.Claims;

namespace Clovance.ApiService.Infrastructure.Auth.Jwt;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(Guid userId, string email, IEnumerable<string> roles);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
