using Clovance.ApiService.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Infrastructure.Database;

public sealed class PostgresTransactionDescriptionSearch(ClovanceDbContext dbContext) : ITransactionDescriptionSearch
{
    public IQueryable<TransactionId> FindMatchingTransactionIds(string searchText)
    {
        var escapedSearchText = EscapeLikePattern(searchText);
        var pattern = $"%{escapedSearchText}%";

        // PostgreSQL ILIKE uses the GIN trigram index while allowing a substring match.
        // Escaping LIKE metacharacters makes the user-provided search text literal.
        return dbContext.Transactions
            .FromSqlInterpolated($@"
                SELECT * FROM transactions
                WHERE description ILIKE {pattern} ESCAPE '!'")
            .AsNoTracking()
            .Select(transaction => transaction.Id);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("!", "!!", StringComparison.Ordinal)
        .Replace("%", "!%", StringComparison.Ordinal)
        .Replace("_", "!_", StringComparison.Ordinal);
}
