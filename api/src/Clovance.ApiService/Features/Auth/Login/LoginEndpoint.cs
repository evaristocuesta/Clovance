using Clovance.ApiService.Infrastructure.Validation;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/login", async (LoginCommand command, LoginCommandHandler handler) =>
        {
            try
            {
                var result = await handler.Handle(command);
                return Results.SignIn(result.Principal, authenticationScheme: IdentityConstants.BearerScheme);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithValidation<LoginCommand>()
        .WithName("Login")
        .WithTags("Auth")
        .WithSummary("Login to get Bearer token")
        .WithDescription("Returns a Bearer token in the response that can be used for authenticated requests. Copy the 'access_token' value from the response.");

        return builder;
    }
}
