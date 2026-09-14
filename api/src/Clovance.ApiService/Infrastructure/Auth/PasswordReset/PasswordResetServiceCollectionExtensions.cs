namespace Clovance.ApiService.Infrastructure.Auth.PasswordReset;

public static class PasswordResetServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordReset(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));
        return services;
    }
}
