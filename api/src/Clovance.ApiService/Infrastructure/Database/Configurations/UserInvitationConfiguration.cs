using Clovance.ApiService.Domain.UserInvitations;
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
            .HasConversion(
                id => id.Value,
                id => UserInvitationId.Create(id))
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                email => UserInvitationEmail.Create(email))
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasConversion(
                token => token.Value,
                token => UserInvitationToken.Create(token))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.ConsumedBy)
            .HasMaxLength(450);

        builder.HasIndex(x => x.Email);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAt);
    }
}
