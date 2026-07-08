using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Features.Accounts.GetAccountById;

public class GetAccountByIdQueryHandler : IHandler<GetAccountByIdQuery, Result<GetAccountByIdResult>>
{
    private readonly ClovanceDbContext _dbContext;

    public GetAccountByIdQueryHandler(ClovanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetAccountByIdResult>> HandleAsync(GetAccountByIdQuery command, CancellationToken cancellationToken)
    {
        var account = await _dbContext
            .Accounts
            .AsNoTracking()
            .Where(a => a.Id == AccountId.Create(command.Id))
            .Select(a => a.ToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            return Result<GetAccountByIdResult>.Failure(AppErrors.Accounts.AccountNotFound());
        }

        return Result<GetAccountByIdResult>.Success(new GetAccountByIdResult(account));
    }
}
