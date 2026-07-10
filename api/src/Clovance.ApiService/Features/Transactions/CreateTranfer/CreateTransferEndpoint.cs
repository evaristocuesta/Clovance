using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.CreateTranfer;

public class CreateTransferEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/transfer", async (
            CreateTransferCommand command,
            IHandler<CreateTransferCommand, Result<CreateTransferResult>> handler,
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
        .Produces<CreateTransferResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithName("CreateTransfer")
        .WithSummary("Create Transfer")
        .WithDescription("Creates a new transfer"); ;
    }
}
