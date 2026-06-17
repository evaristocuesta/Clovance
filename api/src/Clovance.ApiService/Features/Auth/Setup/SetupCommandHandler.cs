using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.RegisterAdmin;

public sealed class SetupCommandHandler : IHandler<SetupCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SetupCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> HandleAsync(SetupCommand command, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.ToList();

        if (users.Any())
        {
            return Result<Result>.Failure(AppErrors.Auth.SetupAlreadyBeenCompleted());
        }

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            EmailConfirmed = true,
            FirstName = command.FirstName,
            LastName = command.LastName,
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Result<Result>.Failure(AppErrors.Auth.UserCreationFailed(errors));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
        
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return Result<Result>.Failure(AppErrors.Auth.UserCreationFailed(errors));
        }

        return Result.Success();
    }
}
