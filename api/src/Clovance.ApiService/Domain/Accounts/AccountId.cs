namespace Clovance.ApiService.Domain.Accounts;

public readonly record struct AccountId(Guid Value)
{
    public static AccountId New()
    {
        return new AccountId(Guid.NewGuid());
    }

    public static AccountId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Account id cannot be empty.", nameof(value));
        }

        return new AccountId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
