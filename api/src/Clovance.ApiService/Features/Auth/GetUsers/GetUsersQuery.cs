namespace Clovance.ApiService.Features.Auth.GetUsers;

public sealed record GetUsersQuery();

public sealed record GetUsersResult(
    IEnumerable<UserDto> Users);
