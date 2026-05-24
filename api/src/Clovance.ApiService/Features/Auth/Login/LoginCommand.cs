using System.Security.Claims;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password);

public sealed record LoginResult(ClaimsPrincipal Principal);
