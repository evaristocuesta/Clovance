namespace Clovance.ApiService.Infrastructure.Auth.UserInvitation;

public sealed class UserInvitationOptions
{
    public const string SectionName = "Identity:Invitations";

    public int ExpirationHours { get; set; } = 48;
}
