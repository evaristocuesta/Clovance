namespace Clovance.ApiService.Infrastructure.Database;

public sealed class IdentityAdminOptions
{
    public const string SectionName = "Identity:Admin";

    public string Email { get; set; } = "admin@clovance.local";

    public string Password { get; set; } = "Change.Me.1234";
}
