namespace Clovance.ApiService.Features.Accounts.CreateAccount;

public sealed record CreateAccountCommand(string Name, string Currency);

public sealed record CreateAccountResult(AccountDto Account);
