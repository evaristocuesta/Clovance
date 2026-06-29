using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.UpdateUser;

public class UpdateUserEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/users", async (
            UpdateUserCommand command,
            IHandler<UpdateUserCommand, Result<UpdateUserResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value);
        })
        .Produces<UpdateUserResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .WithName("UpdateUser")
        .WithSummary("Update User")
        .WithDescription("Updates the details of an existing user.");
    }
}
