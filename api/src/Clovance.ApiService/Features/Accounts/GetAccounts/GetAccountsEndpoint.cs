using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.GetAccounts;

public class GetAccountsEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            IHandler<GetAccountsQuery, Result<GetAccountsResult>> handler, 
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetAccountsQuery(), cancellationToken);
            return Results.Ok(result.Value.Accounts);
        })
        .Produces<List<AccountDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetAccounts")
        .WithSummary("Get Accounts")
        .WithDescription("Retrieve a list of all accounts.");
    }
}
