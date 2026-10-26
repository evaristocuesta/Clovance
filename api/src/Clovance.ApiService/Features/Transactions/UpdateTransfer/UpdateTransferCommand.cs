namespace Clovance.ApiService.Features.Transactions.UpdateTransfer;

public sealed record UpdateTransferRequest(
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid FromAccountId,
    Guid ToAccountId,
    uint Version,
    uint RelatedVersion);

public sealed record UpdateTransferCommand(
    Guid TransactionId,
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid FromAccountId,
    Guid ToAccountId,
    uint Version,
    uint RelatedVersion);

public sealed record UpdateTransferResult(
    TransactionDto FromTransaction,
    TransactionDto ToTransaction);
