namespace Clovance.ApiService.Features.Auth.GetUsers;

public sealed record GetUsersRequest();

public sealed record GetUsersResult(
    IEnumerable<UserDto> Users);
