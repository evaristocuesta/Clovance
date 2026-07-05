using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public class UpdateAccountEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}", async (
            UpdateAccountRequest request, 
            IHandler<UpdateAccountCommand, Result<UpdateAccountResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken, 
            Guid id) =>
        {
            var command = new UpdateAccountCommand(
                Id: id,
                Name: request.Name,
                Currency: request.Currency
            );

            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value.Account);
        })
        .Produces<AccountDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("UpdateAccount")
        .WithSummary("Update Account")
        .WithDescription("Updates the details of an existing account identified by its ID.");
    }
}
