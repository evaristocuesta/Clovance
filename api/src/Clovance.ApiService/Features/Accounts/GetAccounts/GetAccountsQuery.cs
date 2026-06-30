namespace Clovance.ApiService.Features.Accounts.GetAccounts;

public sealed record GetAccountsQuery();

public sealed record GetAccountsQueryResult(IEnumerable<AccountDto> Accounts);


