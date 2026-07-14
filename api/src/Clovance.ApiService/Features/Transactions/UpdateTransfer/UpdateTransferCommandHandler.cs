using System.Security.Claims;
using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Features.Transactions.UpdateTransaction;
using Clovance.ApiService.Infrastructure.Database;

namespace Clovance.ApiService.Features.Transactions.UpdateTransfer;

public class UpdateTransferCommandHandler : IHandler<UpdateTransferCommand, Result<UpdateTransferResult>>
{
    private readonly ClovanceDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public UpdateTransferCommandHandler(ClovanceDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result<UpdateTransferResult>> HandleAsync(UpdateTransferCommand command, CancellationToken cancellationToken)
    {
        var transaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(command.TrasactionId), cancellationToken);

        if (transaction is null || !transaction.RelatedTransactionId.HasValue)
        {
            return Result<UpdateTransferResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        var relatedTransaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(transaction.RelatedTransactionId.Value.Value), cancellationToken);

        if (relatedTransaction is null)
        {
            return Result<UpdateTransferResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        var userId = Guid.TryParse(
            _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId) ?
                parsedUserId :
                Guid.Empty;

        if (userId == Guid.Empty)
        {
            return Result<UpdateTransferResult>.Failure(AppErrors.Auth.UserNotAuthenticated());
        }

        var fromTransaction = transaction.Amount.Value < 0 ? transaction : relatedTransaction;

        fromTransaction.ChangeDate(TransactionDate.Create(command.Date), userId);
        fromTransaction.ChangeAmount(TransactionAmount.Create(Math.Abs(command.Amount) * -1), userId);
        fromTransaction.ChangeDescription(TransactionDescription.Create(command.Description), userId);
        fromTransaction.MoveToAccount(AccountId.Create(command.FromAccountId), userId);

        _context.Transactions.Update(fromTransaction);

        var toTransaction = transaction.Amount.Value < 0 ? relatedTransaction : transaction;

        toTransaction.ChangeDate(TransactionDate.Create(command.Date), userId);
        toTransaction.ChangeAmount(TransactionAmount.Create(Math.Abs(command.Amount)), userId);
        toTransaction.ChangeDescription(TransactionDescription.Create(command.Description), userId);
        toTransaction.MoveToAccount(AccountId.Create(command.ToAccountId), userId);

        _context.Transactions.Update(relatedTransaction);

        _context.SaveChanges();

        return Result<UpdateTransferResult>.Success(new UpdateTransferResult
        (
            FromTransaction: fromTransaction.ToDto(),
            ToTransaction: toTransaction.ToDto()
        ));
    }
}
