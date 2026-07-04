using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.GetCurrencies;

public class GetCurrenciesEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/currencies", async (IHandler<GetCurrenciesQuery, GetCurrenciesResult> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCurrenciesQuery(), cancellationToken);
            return Results.Ok(result.Currencies);
        })
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("GetCurrencies")
        .WithSummary("Get Currencies")
        .WithDescription("Retrieves a list of available currencies.");
    }
}
