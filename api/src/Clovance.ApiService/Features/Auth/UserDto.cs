namespace Clovance.ApiService.Features.Auth;

public sealed record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    IEnumerable<string> Roles);
