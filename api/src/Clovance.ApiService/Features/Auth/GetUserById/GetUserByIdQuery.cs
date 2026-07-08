namespace Clovance.ApiService.Features.Auth.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId);

public sealed record GetUserByIdResult(UserDto User);
