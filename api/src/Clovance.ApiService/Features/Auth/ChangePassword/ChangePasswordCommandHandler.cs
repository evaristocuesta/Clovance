using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Result<Result>.Failure(AppErrors.Auth.UserNotFound());
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (changePasswordResult.Succeeded)
        {
            return Result.Success();
        }
        else
        {
            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return Result<Result>.Failure(AppErrors.Auth.PasswordChangeFailed(errors));
        }
    }
}
