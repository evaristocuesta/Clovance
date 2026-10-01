using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Clovance.UnitTests.Features.Accounts;

public abstract class AccountHandlerTestBase : FeatureTestBase
{
    protected ClovanceDbContext Context { get; }
    protected IHttpContextAccessor HttpContextAccessor { get; }

    protected AccountHandlerTestBase()
    {
        Context = TestDbContextFactory.CreateInMemoryDbContext();
        HttpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    protected void Authenticate(Guid userId)
    {
        FeatureTestBase.Authenticate(HttpContextAccessor, userId);
    }

    protected static Account CreateAccount(string name, AccountType type, string currency, Guid? createdBy = null)
    {
        return Account.Create(name, type, currency, createdBy ?? Guid.CreateVersion7());
    }
}
