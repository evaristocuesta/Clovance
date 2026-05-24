namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed record CreateInvitationCommand(string Email);

public sealed record CreateInvitationResult(
    Guid Id,
    string Email,
    DateTimeOffset ExpiresAt,
    string Token);
