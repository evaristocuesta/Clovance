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

        // xmin is a PostgreSQL system column, not a physical column of the table: Npgsql
        // recognises the name and leaves it out of migration operations. It changes on every
        // update to the row, which is what lets EF detect a lost update instead of silently
        // overwriting a concurrent change. Npgsql maps it to uint.
        builder.Property(x => x.Xmin)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.Name)
            .HasConversion(name => name.Value, value => AccountName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<string>() // save "Checking"/"Savings"/"Cash"/"CreditCard"/"Loan"/"Mortgage"/"Investment" instead of 0/1/2, more readable in the DB
            .HasMaxLength(20)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_transactions_type_is_valid",
                "type IN ('Checking', 'Savings', 'Cash', 'CreditCard', 'Loan', 'Mortgage', 'Investment')");
        });

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
