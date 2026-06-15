using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.SetupCompleted;

public class SetupCompletedEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/setup-completed", async (
            IHandler<SetupCompletedQuery, Result<SetupCompletedResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new SetupCompletedQuery(), cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value);
        })
        .Produces<SetupCompletedResult>(StatusCodes.Status200OK)
        .WithName("SetupCompleted")
        .WithSummary("Setup completed")
        .WithDescription("Checks if the setup process has been completed.");
    }
}
