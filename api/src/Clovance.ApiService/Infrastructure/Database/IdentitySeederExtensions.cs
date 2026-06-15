using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Infrastructure.Database;

public static class IdentitySeederExtensions
{
    public static async Task SeedIdentityAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }
}
