using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Transactions.GetTransactions;

public class GetTransactionsQueryHandler : IHandler<GetTransactionsQuery, Result<GetTransactionsPageResponse>>
{
    private readonly ClovanceDbContext _dbContext;

    public GetTransactionsQueryHandler(ClovanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetTransactionsPageResponse>> HandleAsync(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Transactions.AsNoTracking().AsQueryable();

        if (request.Year.HasValue && request.Month.HasValue)
        {
            var startDate = TransactionDate.Create(new DateOnly(request.Year.Value, request.Month.Value, 1));
            var endDate = TransactionDate.Create(startDate.Value.AddMonths(1));
            query = query.Where(t => t.Date >= startDate && t.Date < endDate);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(t => t.AccountId == AccountId.Create(request.AccountId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var term = request.Description.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Description.Value, $"%{term}%"));
        }

        // Stable order: date desc, and as a tiebreaker Id desc (to ensure the cursor is unique)
        query = query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);

        if (request.CursorDate.HasValue && request.CursorId.HasValue)
        {
            var cursorDate = TransactionDate.Create(request.CursorDate.Value);
            var cursorId = TransactionId.Create(request.CursorId.Value);
            query = query.Where(t =>
                t.Date < cursorDate ||
                (t.Date == cursorDate && t.Id < cursorId));
        }

        // Request one more to know if there are more pages, without doing a separate Count
        var rows = await query
            .Take(request.PageSize + 1)
            .Join(
                _dbContext.Accounts.AsNoTracking(),
                transaction => transaction.AccountId,
                account => account.Id,
                (transaction, account) => transaction.ToDto(account.Name.Value, account.Currency.Code)
             )
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.PageSize;
        var items = hasMore ? rows.Take(request.PageSize).ToList() : rows;

        DateOnly? nextCursorDate = null;
        Guid? nextCursorId = null;

        if (hasMore)
        {
            var last = items[^1];
            nextCursorDate = last.Date;
            nextCursorId = last.Id;
        }

        var response = new GetTransactionsPageResponse(items, hasMore, nextCursorDate, nextCursorId);

        return Result<GetTransactionsPageResponse>.Success(response);
    }
}
