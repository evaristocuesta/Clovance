namespace Clovance.ApiService.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password);

public sealed record LoginResult(string AccessToken, DateTimeOffset ExpiresAt);
