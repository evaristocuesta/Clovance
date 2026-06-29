namespace Clovance.ApiService.Features.Auth.UpdateUser;

public sealed record UpdateUserCommand(string Email, string FirstName, string LastName);
public sealed record UpdateUserResult(string Token);
