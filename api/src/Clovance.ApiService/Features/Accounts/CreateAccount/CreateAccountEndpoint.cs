using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Accounts.CreateAccount;

public class CreateAccountEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateAccountCommand command, 
            IHandler<CreateAccountCommand, Result<CreateAccountResult>> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblemResult(httpContext);
            }

            return Results.Ok(result.Value.Account);
        })
        .Produces<AccountDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithName("CreateAccount")
        .WithSummary("Create Account")
        .WithDescription("Creates a new account with the specified details.");
    }
}
