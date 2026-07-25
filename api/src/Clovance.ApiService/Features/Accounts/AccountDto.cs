using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Accounts;

public sealed record AccountDto(Guid Id, string Name, AccountType Type, string Currency, bool? IsDeleted);
