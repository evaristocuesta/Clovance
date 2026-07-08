namespace Clovance.ApiService.Features.Transactions;

public sealed record TransactionDto(
    Guid Id,
    DateOnly Date,
    string Description,
    decimal Amount,
    Guid AccountId);
