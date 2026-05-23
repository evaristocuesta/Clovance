using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Clovance.ApiService.Infrastructure.Database;

public static class IdentitySeederExtensions
{
    private const string AdminRole = "Admin";

    public static async Task SeedIdentityAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentityAdminOptions>>().Value;

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync(AdminRole);
        if (existingAdmins.Count > 0)
        {
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(options.Email);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                MustCompleteOnboarding = true
            };

            var createResult = await userManager.CreateAsync(adminUser, options.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Admin user seed failed: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}
