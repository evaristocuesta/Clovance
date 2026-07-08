using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Infrastructure.Database;

public static class IdentitySeederExtensions
{
    private const string AdminRole = "Admin";

    public static async Task SeedIdentityAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(AdminRole));
        }
    }
}
