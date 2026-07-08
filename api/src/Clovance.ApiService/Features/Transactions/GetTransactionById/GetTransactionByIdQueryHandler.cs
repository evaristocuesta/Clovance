using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.GetTransactionById;

public class GetTransactionByIdQueryHandler : IHandler<GetTransactionByIdQuery, Result<GetTransactionByIdResult>>
{
    private readonly ClovanceDbContext _dbContext;
    
    public GetTransactionByIdQueryHandler(ClovanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetTransactionByIdResult>> HandleAsync(GetTransactionByIdQuery command, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Transactions
            .Where(t => t.Id == TransactionId.Create(command.Id))
            .Select(t => t.ToDto())
            .FirstOrDefaultAsync(cancellationToken);
;

        if (transaction is null)
        {
            return Result<GetTransactionByIdResult>.Failure(AppErrors.Transactions.TransactionNotFound());
        }

        return Result<GetTransactionByIdResult>.Success(new GetTransactionByIdResult(transaction));
    }
}
