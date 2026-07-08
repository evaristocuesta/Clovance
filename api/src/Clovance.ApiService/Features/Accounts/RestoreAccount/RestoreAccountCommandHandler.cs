using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Accounts.CreateAccount;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.RestoreAccount;

public class RestoreAccountCommandHandler : IHandler<RestoreAccountCommand, Result>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public RestoreAccountCommandHandler(
        ClovanceDbContext context, 
        IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> HandleAsync(RestoreAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _context
            .Accounts
            .FindAsync(AccountId.Create(command.Id), cancellationToken);

        if (account is null)
        {
            return Result.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        account.Restore(userId);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
