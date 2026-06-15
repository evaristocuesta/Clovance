namespace Clovance.ApiService.Features.Auth;

public sealed record UserDto(
    string Id,
    string Email,
    IEnumerable<string> Roles);
