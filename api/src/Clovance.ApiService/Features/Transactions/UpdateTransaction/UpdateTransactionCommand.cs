namespace Clovance.ApiService.Features.Transactions.UpdateTransaction;

public sealed record UpdateTransactionRequest(DateOnly Date, string Description, decimal Amount, Guid AccountId);

public sealed record UpdateTransactionCommand(TransactionDto Transaction);

public sealed record UpdateTransactionResult(TransactionDto Transaction);
