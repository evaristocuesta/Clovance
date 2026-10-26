using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest(string Name, AccountType Type, string Currency, uint Version);

public sealed record UpdateAccountCommand(Guid Id, string Name, AccountType Type, string Currency, uint Version);

public sealed record UpdateAccountResult(AccountDto Account);
