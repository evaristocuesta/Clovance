namespace Clovance.ApiService.Infrastructure.Auth.Token;

public interface ITokenService
{
    string GenerateToken();
    string HashToken(string token);
}
