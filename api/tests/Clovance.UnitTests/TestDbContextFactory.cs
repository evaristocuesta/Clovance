using Clovance.ApiService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Clovance.UnitTests;

public class TestDbContextFactory
{
    public static ClovanceDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ClovanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ClovanceDbContext(options);
    }
}
