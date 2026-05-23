using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Infrastructure.Database;

public sealed class ApplicationUser : IdentityUser
{
    public bool MustCompleteOnboarding { get; set; }
}
