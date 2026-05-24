using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IHandler<LogoutCommand, Unit>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Unit> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available.");

        await httpContext.SignOutAsync(IdentityConstants.BearerScheme);

        return Unit.Value;
    }
}
