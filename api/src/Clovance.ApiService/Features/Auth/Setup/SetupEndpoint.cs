using Clovance.ApiService.Features.Auth.RegisterAdmin;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Setup;

public class SetupEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/setup", async (
            SetupCommand command,
            IHandler<SetupCommand, Result> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithName("Setup")
        .WithSummary("Setup")
        .WithDescription("Setup the application with an initial admin user.");
    }
}
