using Clovance.ApiService.Domain.Shared;

namespace Clovance.ApiService.Domain.UserInvitations;

public sealed class UserInvitation : AuditableEntityBase<UserInvitationId>
{
    private UserInvitation()
    {
    }

    private UserInvitation(
      string email,
      bool isAdmin,
      string tokenHash,
      DateTimeOffset expiresAt,
      string createdBy)
    {
        Id = UserInvitationId.New();
        Email = UserInvitationEmail.Create(email);
        IsAdmin = isAdmin;
        TokenHash = UserInvitationToken.Create(tokenHash);
        ExpiresAt = expiresAt;
        MarkAsCreated(createdBy);
    }

    public UserInvitationEmail Email { get; private set; } = null!;

    public bool IsAdmin { get; private set; }

    public UserInvitationToken TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public string? ConsumedBy { get; private set; }

    public bool IsConsumed => ConsumedAt is not null;

    public static UserInvitation Create(
      string email,
      bool isAdmin,
      string tokenHash,
      DateTimeOffset expiresAt,
      string createdBy)
    {
        return new UserInvitation(email, isAdmin, tokenHash, expiresAt, createdBy);
    }

    public void Consume(string consumedBy)
    {
        if (IsConsumed)
        {
            throw new InvalidOperationException("Invitation has already been consumed.");
        }

        ConsumedBy = consumedBy;
        ConsumedAt = DateTimeOffset.UtcNow;
        MarkAsModified(consumedBy);
    }
}
