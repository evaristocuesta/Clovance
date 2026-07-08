using Clovance.ApiService.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => AccountId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasConversion(name => name.Value, value => AccountName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(currency => currency.Code, value => Currency.Create(value))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(100);
    }
}
