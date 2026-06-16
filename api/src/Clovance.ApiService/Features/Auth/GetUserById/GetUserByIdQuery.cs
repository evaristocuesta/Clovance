namespace Clovance.ApiService.Features.Auth.GetUserById;

public sealed record GetUserByIdQuery(string UserId);

public sealed record GetUserByIdResult(UserDto User);
