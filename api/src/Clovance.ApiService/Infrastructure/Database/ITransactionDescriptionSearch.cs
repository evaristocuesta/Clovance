using Clovance.ApiService.Domain.Transactions;

namespace Clovance.ApiService.Infrastructure.Database;

public interface ITransactionDescriptionSearch
{
    /// <summary>
    /// Returns a composable query for transactions whose descriptions contain the search text literally.
    /// The query remains deferred so callers can keep filtering and paging in the database.
    /// </summary>
    IQueryable<TransactionId> FindMatchingTransactionIds(string searchText);
}
