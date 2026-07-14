using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.UpdateTransfer;

public class UpdateTransferEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/transfer/{transactionId:guid}", async (
            Guid transactionId,
            UpdateTransferRequest request,
            IHandler<UpdateTransferCommand, Result<UpdateTransferResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTransferCommand
            (
                TrasactionId: transactionId,
                Date: request.Date,
                Description: request.Description,
                Amount: request.Amount,
                FromAccountId: request.FromAccountId,
                ToAccountId: request.ToAccountId
            );

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemResult(httpContext);
        })
        .Produces<UpdateTransferResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithName("UpdateTransfer")
        .WithSummary("Update Transfer")
        .WithDescription("Updates an existing transfer");
    }
}
