namespace Clovance.ApiService.Features.Transactions.UpdateTransfer;

public sealed record UpdateTransferRequest(
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record UpdateTransferCommand(
    Guid TrasactionId,
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid FromAccountId,
    Guid ToAccountId);

public sealed record UpdateTransferResult(
    TransactionDto FromTransaction,
    TransactionDto ToTransaction);
