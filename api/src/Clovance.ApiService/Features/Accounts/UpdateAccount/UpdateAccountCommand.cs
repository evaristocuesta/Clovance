using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest(string Name, AccountType Type, string Currency);

public sealed record UpdateAccountCommand(Guid Id, string Name, AccountType Type, string Currency);

public sealed record UpdateAccountResult(AccountDto Account);
