using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.GetUserById;

public class GetUserByIdEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (
            IHandler<GetUserByIdQuery, Result<GetUserByIdResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken,
            Guid id) =>
        {
            var result = await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value);
        })
        .Produces<GetUserByIdResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("GetUserById")
        .WithSummary("Get user by Id")
        .WithDescription("Get a user by its Id.");
    }
}
