using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.DeleteUser;

public class DeleteUserEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{id}", async (
            string id,
            IHandler<DeleteUserCommand, Result> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteUserCommand(id), cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("DeleteUser")
        .WithSummary("Delete User")
        .WithDescription("Deletes a user by their ID.");
    }
}
