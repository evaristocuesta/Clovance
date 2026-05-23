namespace Clovance.ApiService.Infrastructure.Database;

public sealed class InvitationOptions
{
    public const string SectionName = "Identity:Invitations";

    public int ExpirationHours { get; set; } = 48;
}
