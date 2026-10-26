using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.UpdateLoanPayment;

public class UpdateLoanPaymentEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/loan-payment/{transactionId:guid}", async (
            Guid transactionId,
            UpdateLoanPaymentRequest request,
            IHandler<UpdateLoanPaymentCommand, Result<UpdateLoanPaymentResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateLoanPaymentCommand
            (
                TransactionId: transactionId,
                Date: request.Date,
                Description: request.Description,
                Amount: request.Amount,
                PrincipalAmount: request.PrincipalAmount,
                FromAccountId: request.FromAccountId,
                ToAccountId: request.ToAccountId,
                Version: request.Version,
                RelatedVersion: request.RelatedVersion
            );
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemResult(httpContext);
        })
            .Produces<UpdateLoanPaymentResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization()
            .WithName("UpdateLoanPayment")
            .WithSummary("Update Loan Payment")
            .WithDescription("Update a loan payment transaction");
    }
}
