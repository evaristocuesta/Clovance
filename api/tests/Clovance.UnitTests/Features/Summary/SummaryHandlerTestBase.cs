using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.ExternalServices;
using NSubstitute;

namespace Clovance.UnitTests.Features.Summary;

public abstract class SummaryHandlerTestBase
{
    protected ClovanceDbContext Context { get; }
    protected ICurrencyConverter CurrencyConverter { get; }

    protected SummaryHandlerTestBase()
    {
        Context = TestDbContextFactory.CreateInMemoryDbContext();
        CurrencyConverter = Substitute.For<ICurrencyConverter>();
        CurrencyConverter
            .GetExchangeRatesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase));
    }

    protected static Account CreateAccount(string name, AccountType type, string currency, Guid? createdBy = null)
    {
        return Account.Create(name, type, currency, createdBy ?? Guid.CreateVersion7());
    }
}
