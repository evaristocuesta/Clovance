using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.DeleteAccount;

public class DeleteAccountCommandHandler : IHandler<DeleteAccountCommand, Result>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public DeleteAccountCommandHandler(
        ClovanceDbContext context, 
        IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> HandleAsync(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _context
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == AccountId.Create(command.Id), cancellationToken);

        if (account is null)
        {
            return Result.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var email = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "system";

        account.SoftDelete(email);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
