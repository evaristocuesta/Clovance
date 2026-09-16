namespace Clovance.ApiService.Infrastructure.Frontend;

public static class FrontendServiceCollectionExtensions
{
    public static IServiceCollection AddFrontend(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        return services;
    }
}
