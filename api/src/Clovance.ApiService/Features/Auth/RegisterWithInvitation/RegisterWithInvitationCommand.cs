namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed record RegisterWithInvitationCommand(
    string Email,
    string Password,
    string Token,
    string FirstName,
    string LastName);

public sealed record RegisterWithInvitationResult(Guid UserId, string Email);
