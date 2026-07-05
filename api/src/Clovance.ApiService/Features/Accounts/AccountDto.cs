namespace Clovance.ApiService.Features.Accounts;

public sealed record AccountDto(Guid Id, string Name, string Currency, bool? IsDeleted);
