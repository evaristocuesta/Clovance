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
            .FindAsync(command.Id, cancellationToken);

        if (transaction is null)
        {
            return Result.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}
