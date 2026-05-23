using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => TransactionId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.AccountId)
            .HasConversion(id => id.Value, value => AccountId.Create(value))
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasConversion(amount => amount.Value, value => TransactionAmount.Create(value))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasConversion(description => description.Value, value => TransactionDescription.Create(value))
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .HasConversion(date => date.Value, value => TransactionDate.Create(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.AccountId, x.TransactionDate });

        builder.HasIndex(x => x.Description)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
