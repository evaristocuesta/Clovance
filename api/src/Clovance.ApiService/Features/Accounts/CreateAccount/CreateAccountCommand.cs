using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Accounts.CreateAccount;

public sealed record CreateAccountCommand(string Name, AccountType Type, string Currency);

public sealed record CreateAccountResult(AccountDto Account);
