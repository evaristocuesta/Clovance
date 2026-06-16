using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.GetUsers;

public class GetUsersEndPoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (
            IHandler<GetUsersQuery, Result<GetUsersResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetUsersQuery(), cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value);
        })
        .Produces<GetUsersResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetUsers")
        .WithSummary("Get Users")
        .WithDescription("Retrieves a list of all users in the system.");
    }
}
