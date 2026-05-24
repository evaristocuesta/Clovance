using Clovance.ApiService.Features.Auth.CompleteOnboarding;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Features.Auth.Logout;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;

namespace Clovance.ApiService.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth");

        authGroup.MapLoginEndpoint();
        authGroup.MapLogoutEndpoint();
        authGroup.MapCreateInvitationEndpoint();
        authGroup.MapRegisterWithInvitationEndpoint();
        authGroup.MapCompleteOnboardingEndpoint();

        return app;
    }
}
