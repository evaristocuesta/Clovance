namespace Clovance.ApiService.Features.Accounts.GetAccounts;

public sealed record GetAccountsQuery();

public sealed record GetAccountsResult(IEnumerable<AccountDto> Accounts);


