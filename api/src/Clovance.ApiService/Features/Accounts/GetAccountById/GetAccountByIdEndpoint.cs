using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.GetAccountById;

public class GetAccountByIdEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:guid}", async (
            Guid id, 
            IHandler<GetAccountByIdQuery, Result<GetAccountByIdResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetAccountByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value.Account) : result.ToProblemResult(httpContext);
        })
        .Produces<AccountDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetAccountById")
        .WithSummary("Get account by ID")
        .WithDescription("Retrieves an account by its unique identifier.");
    }
}
