namespace Clovance.ApiService.Infrastructure.Auth.UserInvitation;

public static class UserInvitationServiceCollectionExtensions
{
    public static IServiceCollection AddUserInvitationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UserInvitationOptions>(configuration.GetSection(UserInvitationOptions.SectionName));
        return services;
    }
}
