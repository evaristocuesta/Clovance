using System.Security.Claims;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.GetCurrentUser;

public class GetCurrentUserQueryHandler : IHandler<GetCurrentUserQuery, Result<GetCurrentUserQueryResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<GetCurrentUserQueryResult>> HandleAsync(GetCurrentUserQuery command, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Result<GetCurrentUserQueryResult>.Failure(AppErrors.Auth.UserNotFound());
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = user.ToDto(roles);

        return Result<GetCurrentUserQueryResult>.Success(new GetCurrentUserQueryResult(userDto));
    }
}
