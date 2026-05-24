using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available.");

        await httpContext.SignOutAsync(IdentityConstants.BearerScheme);
    }
}
