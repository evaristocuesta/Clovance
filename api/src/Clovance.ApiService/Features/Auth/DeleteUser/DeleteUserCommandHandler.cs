using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.DeleteUser;

public class DeleteUserCommandHandler : IHandler<DeleteUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);

        if (user is null)
        {
            return Result.Failure(AppErrors.Auth.UserNotFound());
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return Result.Failure(AppErrors.Auth.UserDeletionFailed());
        }

        return Result.Success();
    }
}
