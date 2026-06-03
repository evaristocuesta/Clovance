using Clovance.ApiService.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => RefreshTokenId.Create(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(userId => userId.Value, value => RefreshTokenUserId.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasConversion(token => token.Value, value => RefreshTokenToken.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.HasKey(x => x.UserId);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAt);
    }
}
