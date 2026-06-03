using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Domain.UserInvitations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clovance.ApiService.Infrastructure.Database;

public sealed class ClovanceDbContext(DbContextOptions<ClovanceDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClovanceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
