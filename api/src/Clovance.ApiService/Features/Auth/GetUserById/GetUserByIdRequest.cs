namespace Clovance.ApiService.Features.Auth.GetUserById;

public sealed record GetUserByIdRequest(string UserId);

public sealed record GetUserByIdResult(UserDto User);
