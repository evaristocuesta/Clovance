using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
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

        builder.Property(t => t.Type)
            .HasConversion<string>() // save "Income"/"Expense"/"Transfer" instead of 0/1/2, more readable in the DB
            .HasMaxLength(20)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_transactions_type_is_valid",
                "type IN ('Income', 'Expense', 'Transfer')");

            t.HasCheckConstraint(
                "ck_transactions_amount_sign_matches_type",
                "(type = 'Income' AND amount > 0) OR " +
                "(type = 'Expense' AND amount < 0) OR " +
                "(type = 'Transfer' AND amount <> 0)");
        });

        builder.Property(x => x.Description)
            .HasConversion(description => description.Value, value => TransactionDescription.Create(value))
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Date)
            .HasConversion(date => date.Value, value => TransactionDate.Create(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.AccountId, x.Date });

        builder.HasIndex(x => x.Description)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Transaction>()
            .WithOne()
            .HasForeignKey<Transaction>(t => t.RelatedTransactionId);
    }
}
