using Clovance.ApiService.Domain.Accounts;

namespace Clovance.ApiService.Features.Accounts;

public static class AccountMappers
{
    public static AccountDto ToDto(
        this Account account)
    {
        return new(
            account.Id.Value,
            account.Name.Value,
            account.Currency.Code);
    }
}
