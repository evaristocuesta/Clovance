using System.Text.Json.Serialization;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password);

public sealed record LoginResult(string AccessToken, DateTimeOffset ExpiresAt);

public sealed record LoginResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt);
