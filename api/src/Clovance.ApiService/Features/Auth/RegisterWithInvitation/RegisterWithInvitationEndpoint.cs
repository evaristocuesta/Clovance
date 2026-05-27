using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/register-with-invitation", async (
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
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithName("RegisterWithInvitation")
        .WithSummary("Register With Invitation")
        .WithDescription("Registers a new user using an invitation.");
    }
}
