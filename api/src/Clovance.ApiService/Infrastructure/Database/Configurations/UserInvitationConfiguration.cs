using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clovance.ApiService.Infrastructure.Database.Configurations;

public sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("user_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.ConsumedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.TokenHash)
            .IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}
