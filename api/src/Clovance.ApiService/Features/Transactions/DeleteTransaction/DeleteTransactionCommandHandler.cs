using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;

namespace Clovance.ApiService.Features.Transactions.DeleteTransaction;

public class DeleteTransactionCommandHandler : IHandler<DeleteTransactionCommand, Result>
{
    private readonly ClovanceDbContext _context;

    public DeleteTransactionCommandHandler(ClovanceDbContext context)
    {
        _context = context;
    }

    public async Task<Result> HandleAsync(DeleteTransactionCommand command, CancellationToken cancellationToken)
    {
        var transaction = await _context
            .Transactions
            .FindAsync(TransactionId.Create(command.Id), cancellationToken);

        if (transaction is null)
        {
            return Result.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        if (transaction.RelatedTransactionId.HasValue)
        {
            await DeleteTransactionWithRelated(transaction, cancellationToken);
            return Result.Success();
        }

        await DeleteTransaction(transaction, cancellationToken);
        return Result.Success();
    }

    private async Task DeleteTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteTransactionWithRelated(Transaction transaction, CancellationToken cancellationToken)
    {
        var relatedTransaction = await _context
            .Transactions
            .FindAsync(transaction.RelatedTransactionId!.Value, cancellationToken);

        await using var transactionScope = await _context.Database.BeginTransactionAsync(cancellationToken);

        transaction.ChangeRelatedTransactionId(null);

        if (relatedTransaction is not null)
        {
            relatedTransaction.ChangeRelatedTransactionId(null);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.Transactions.Remove(transaction);

        if (relatedTransaction is not null)
        {
            _context.Transactions.Remove(relatedTransaction);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transactionScope.CommitAsync(cancellationToken);
    }
}
