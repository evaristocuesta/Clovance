using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Accounts.UpdateAccount;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Transactions.GetTransactionById;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.UpdateTransaction;

public class UpdateTransactionCommandHandler : IHandler<UpdateTransactionCommand, Result<UpdateTransactionResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public UpdateTransactionCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<UpdateTransactionResult>> HandleAsync(UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        var transaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(command.Transaction.Id), cancellationToken);

        if (transaction is null)
        {
            return Result<UpdateTransactionResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        var account = await _context
            .Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == AccountId.Create(command.Transaction.AccountId), cancellationToken);

        if (account is null)
        {
            return Result<UpdateTransactionResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<UpdateTransactionResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        transaction.ChangeDate(TransactionDate.Create(command.Transaction.Date), userId);
        transaction.ChangeAmount(TransactionAmount.Create(command.Transaction.Amount), userId);
        transaction.ChangeDescription(TransactionDescription.Create(command.Transaction.Description), userId);
        transaction.MoveToAccount(AccountId.Create(command.Transaction.AccountId), userId);

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UpdateTransactionResult>.Success(new UpdateTransactionResult(transaction.ToDto()));
    }
}
