using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Clovance.ApiService.Infrastructure.Database;

public sealed class PostgresSetupTransactionProvider(ClovanceDbContext dbContext) : ISetupTransactionProvider
{
    private const long SetupAdvisoryLockKey = 0x434C4F56414E4345L;

    public async Task<IDbContextTransaction> BeginSetupTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({SetupAdvisoryLockKey})",
                cancellationToken);

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}
