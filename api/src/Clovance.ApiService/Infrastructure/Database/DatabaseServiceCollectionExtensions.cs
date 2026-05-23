using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityAdminOptions>(configuration.GetSection(IdentityAdminOptions.SectionName));
        services.Configure<InvitationOptions>(configuration.GetSection(InvitationOptions.SectionName));
        services.AddSingleton<IInvitationTokenService, InvitationTokenService>();

        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ClovanceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
          .AddIdentityCore<ApplicationUser>(options =>
          {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
          })
          .AddRoles<IdentityRole>()
          .AddSignInManager()
          .AddEntityFrameworkStores<ClovanceDbContext>()
          .AddDefaultTokenProviders();

        return services;
    }
}
