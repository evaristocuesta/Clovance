using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.UpdateUser;

public class UpdateUserCommandHandler : IHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtTokenService _jwtTokenService;

    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<UpdateUserResult>> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Result<UpdateUserResult>.Failure(AppErrors.Auth.UserNotFound());
        }

        user.UserName = command.Email;
        user.Email = command.Email;
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
        {
            var token = _jwtTokenService.GenerateToken(
                user.Id, 
                user.Email ?? string.Empty,
                await _userManager.GetRolesAsync(user));
            
            return Result<UpdateUserResult>.Success(new UpdateUserResult(token.Token));
        }
        else
        {
            return Result<UpdateUserResult>.Failure(AppErrors.Auth.UserUpdateFailed());
        }
    }
}
