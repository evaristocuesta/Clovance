namespace Clovance.ApiService.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery();

public sealed record GetCurrentUserQueryResult(UserDto CurrentUser);
