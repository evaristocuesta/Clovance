using Clovance.ApiService.Infrastructure.UserInvitations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UserInvitationOptions>(configuration.GetSection(UserInvitationOptions.SectionName));

        var connectionString = configuration.GetConnectionString("clovance-database");

        services.AddDbContext<ClovanceDbContext>(options =>
        {
            options
                .UseNpgsql(
                    connectionString, 
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("ef_migrations_history");
                    })
                .UseSnakeCaseNamingConvention();
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
          .AddRoles<IdentityRole<Guid>>()
          .AddSignInManager()
          .AddEntityFrameworkStores<ClovanceDbContext>()
          .AddDefaultTokenProviders();

        return services;
    }
}
