using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Transactions.CreateLoanPayment;

public class CreateLoanPaymentEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/loan-payment", async (
            CreateLoanPaymentCommand command,
            IHandler<CreateLoanPaymentCommand, Result<CreateLoanPaymentResult>> handler,
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
        .Produces<CreateLoanPaymentResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithName("CreateLoanPayment")
        .WithSummary("Create Loan Payment")
        .WithDescription("Creates a new loan payment");
    }
}
