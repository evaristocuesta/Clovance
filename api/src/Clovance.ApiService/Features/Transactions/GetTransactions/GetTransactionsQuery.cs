namespace Clovance.ApiService.Features.Transactions.GetTransactions;

public sealed record GetTransactionsQuery(
    int? Year,
    int? Month,
    Guid? AccountId,
    string? Description,
    DateOnly? CursorDate,
    Guid? CursorId,
    int PageSize = 30);

public sealed record GetTransactionsPageResponse(
    IReadOnlyList<TransactionDto> Items,
    bool HasMore,
    DateOnly? NextCursorDate,
    Guid? NextCursorId);
