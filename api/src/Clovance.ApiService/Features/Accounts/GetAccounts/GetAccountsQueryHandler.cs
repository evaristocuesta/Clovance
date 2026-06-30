using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.GetAccounts;

public class GetAccountsQueryHandler : IHandler<GetAccountsQuery, GetAccountsQueryResult>
{
    private readonly ClovanceDbContext _dbContext;

    public GetAccountsQueryHandler(ClovanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAccountsQueryResult> HandleAsync(GetAccountsQuery command, CancellationToken cancellationToken)
    {
        var accounts = await _dbContext.Accounts
            .Select(a => a.ToDto())
            .ToListAsync(cancellationToken);

        return new GetAccountsQueryResult(accounts);
    }
}
