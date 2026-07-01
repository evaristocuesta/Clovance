namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest(string Name, string Currency);

public sealed record UpdateAccountCommand(AccountDto Account);

public sealed record UpdateAccountResult(AccountDto Account);
