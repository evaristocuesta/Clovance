using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.SetupCompleted;

public class SetupCompletedQueryHandler : IHandler<SetupCompletedQuery, Result<SetupCompletedResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SetupCompletedQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<SetupCompletedResult>> HandleAsync(SetupCompletedQuery command, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.ToList();
        return Result<SetupCompletedResult>.Success(new SetupCompletedResult(users.Any()));
    }
}
