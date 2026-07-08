using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Auth.CreateInvitation;
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
            .FirstOrDefaultAsync(a => a.Id == AccountId.Create(command.Id), cancellationToken);

        if (account is null)
        {
            return Result<UpdateAccountResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<UpdateAccountResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        account.Rename(command.Name, userId);
        account.ChangeCurrency(command.Currency, userId);

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateAccountResult>.Success(new UpdateAccountResult(account.ToDto()));
    }
}
