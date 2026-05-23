using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.Accounts;

public sealed class Currency : ValueObject
{
  private Currency(string code)
  {
    Code = code;
  }

  public string Code { get; }

  public static Currency Create(string code)
  {
    if (string.IsNullOrWhiteSpace(code))
    {
      throw new ArgumentException("Currency code is required.", nameof(code));
    }

    var normalizedCode = code.Trim().ToUpperInvariant();

    if (normalizedCode.Length != 3)
    {
      throw new ArgumentException("Currency code must contain 3 letters.", nameof(code));
    }

    if (!normalizedCode.All(char.IsLetter))
    {
      throw new ArgumentException("Currency code must contain only letters.", nameof(code));
    }

    return new Currency(normalizedCode);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Code;
  }

  public override string ToString()
  {
    return Code;
  }
}
