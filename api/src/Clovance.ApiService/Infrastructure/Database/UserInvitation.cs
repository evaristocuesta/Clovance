namespace Clovance.ApiService.Infrastructure.Database;

public sealed class UserInvitation
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTimeOffset? ConsumedAt { get; set; }

    public string? ConsumedByUserId { get; set; }

    public bool IsConsumed => ConsumedAt is not null;
}
