using Microsoft.EntityFrameworkCore.Storage;

namespace Clovance.ApiService.Infrastructure.Database;

public interface ISetupTransactionProvider
{
    Task<IDbContextTransaction> BeginSetupTransactionAsync(CancellationToken cancellationToken);
}
