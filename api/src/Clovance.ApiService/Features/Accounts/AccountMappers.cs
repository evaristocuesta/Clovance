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
            account.Currency.Code, 
            account.IsDeleted);
    }

    public static Account ToDomain(
        this AccountDto accountDto, 
        Guid createdBy)
    {
        return Account.Create(
            AccountName.Create(accountDto.Name), 
            Currency.Create(accountDto.Currency), 
            createdBy);
    }
}
