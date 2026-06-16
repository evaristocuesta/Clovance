using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.GetCurrentUser;

public class GetCurrentUserEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", async (
            HttpContext httpContext,
            IHandler<GetCurrentUserQuery, Result<GetCurrentUserQueryResult>> handler, 
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCurrentUserQuery(), cancellationToken);
            
            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }
            
            return Results.Ok(result.Value.CurrentUser);
        })
        .Produces<GetCurrentUserQueryResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Get Current User")
        .WithDescription("Retrieves the currently authenticated user's information.");
    }
}
