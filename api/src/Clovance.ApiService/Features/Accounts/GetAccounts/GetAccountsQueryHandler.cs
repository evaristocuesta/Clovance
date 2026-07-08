using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.GetAccounts;

public class GetAccountsQueryHandler : IHandler<GetAccountsQuery, Result<GetAccountsResult>>
{
    private readonly ClovanceDbContext _dbContext;

    public GetAccountsQueryHandler(ClovanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetAccountsResult>> HandleAsync(GetAccountsQuery command, CancellationToken cancellationToken)
    {
        var accounts = await _dbContext
            .Accounts
            .AsNoTracking()
            .Select(a => a.ToDto())
            .ToListAsync(cancellationToken);

        return Result<GetAccountsResult>.Success(new GetAccountsResult(accounts));
    }
}
