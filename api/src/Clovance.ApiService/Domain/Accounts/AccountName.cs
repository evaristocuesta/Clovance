using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Accounts;

public sealed class AccountName : ValueObject
{
  private AccountName(string value)
  {
    Value = value;
  }

  public string Value { get; }

  public static AccountName Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("Account name is required.", nameof(value));
    }

    return new AccountName(value.Trim());
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString()
  {
    return Value;
  }
}
