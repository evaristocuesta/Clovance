using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public class UpdateAccountCommandHandler : IHandler<UpdateAccountCommand, Result<UpdateAccountResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public UpdateAccountCommandHandler(
        ClovanceDbContext context, 
        IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<UpdateAccountResult>> HandleAsync(UpdateAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _context
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == AccountId.Create(command.Account.Id), cancellationToken);

        if (account is null)
        {
            return Result<UpdateAccountResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var email = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "system";

        account.Rename(command.Account.Name, email);
        account.ChangeCurrency(command.Account.Currency, email);

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateAccountResult>.Success(new UpdateAccountResult(account.ToDto()));
    }
}
