namespace Clovance.ApiService.Features.Accounts.GetAccountById;

public sealed record GetAccountByIdQuery(Guid Id);

public sealed record GetAccountByIdResult(AccountDto? Account);
