using Clovance.ApiService.Domain.PasswordResetTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => PasswordResetTokenId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(userId => userId.Value, value => PasswordResetTokenUserId.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasConversion(hash => hash.Value, value => PasswordResetTokenHash.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAt);
    }
}
