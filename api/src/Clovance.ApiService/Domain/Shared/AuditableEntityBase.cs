namespace Clovance.ApiService.Domain.Shared;

public abstract class AuditableEntityBase<TId> : EntityBase<TId>
  where TId : struct, IEquatable<TId>
{
  public DateTimeOffset CreatedAt { get; protected set; }

  public string CreatedBy { get; protected set; } = string.Empty;

  public DateTimeOffset? ModifiedAt { get; protected set; }

  public string? ModifiedBy { get; protected set; }

  protected void MarkAsCreated(string createdBy, DateTimeOffset? createdAt = null)
  {
    if (string.IsNullOrWhiteSpace(createdBy))
    {
      throw new ArgumentException("Creator is required.", nameof(createdBy));
    }

    CreatedBy = createdBy;
    CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
  }

  protected void MarkAsModified(string modifiedBy, DateTimeOffset? modifiedAt = null)
  {
    if (string.IsNullOrWhiteSpace(modifiedBy))
    {
      throw new ArgumentException("Modifier is required.", nameof(modifiedBy));
    }

    ModifiedBy = modifiedBy;
    ModifiedAt = modifiedAt ?? DateTimeOffset.UtcNow;
  }
}
