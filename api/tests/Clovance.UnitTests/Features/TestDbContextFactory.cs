using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clovance.UnitTests.Features;

public class TestDbContextFactory
{
    public static ClovanceDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ClovanceDbContext>()
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
            .Options;

        return new ClovanceDbContext(options);
    }
}
