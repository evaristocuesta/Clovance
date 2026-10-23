using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        var isSetupCompleted = await _userManager.Users.AnyAsync(cancellationToken);
        return Result<SetupCompletedResult>.Success(new SetupCompletedResult(isSetupCompleted));
    }
}
