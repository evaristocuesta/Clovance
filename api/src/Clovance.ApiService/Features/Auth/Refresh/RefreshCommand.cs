namespace Clovance.ApiService.Features.Auth.Refresh;

public sealed record RefreshCommand();

public sealed record RefreshResult(string Token, DateTimeOffset ExpiresAt);
