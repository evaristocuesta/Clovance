using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.ChangePassword;

public class ChangePasswordEndpoint : IApiEndPoint
{
    public void MapApiEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("users/password", async (
            ChangePasswordCommand command, 
            IHandler<ChangePasswordCommand, Result> handler,
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
        .RequireAuthorization()
        .WithName("ChangePassword")
        .WithSummary("Change Password")
        .WithDescription("Changes the password of an existing user.");
    }
}
