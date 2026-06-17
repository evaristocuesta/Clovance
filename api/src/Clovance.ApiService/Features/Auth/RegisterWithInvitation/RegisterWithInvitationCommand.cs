namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed record RegisterWithInvitationCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Token);

public sealed record RegisterWithInvitationResult(string UserId, string Email);
