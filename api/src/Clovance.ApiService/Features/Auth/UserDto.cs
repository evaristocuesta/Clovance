namespace Clovance.ApiService.Features.Auth;

public sealed record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    IEnumerable<string> Roles);
