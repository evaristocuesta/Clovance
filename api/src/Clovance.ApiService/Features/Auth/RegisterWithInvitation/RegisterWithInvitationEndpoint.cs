using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.RateLimiting;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/register", async (
            RegisterWithInvitationCommand command,
            IHandler<RegisterWithInvitationCommand, Result<RegisterWithInvitationResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Created($"/auth/users/{result.Value.UserId}", result.Value);
        })
        .Produces<RegisterWithInvitationResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitingServiceCollectionExtensions.AuthPolicyName)
        .WithName("RegisterWithInvitation")
        .WithSummary("Register With Invitation")
        .WithDescription("Registers a new user using an invitation.");
    }
}
