namespace Clovance.ApiService.Domain.Shared;

public abstract class SoftDeletableAuditableEntityBase<TId> : AuditableEntityBase<TId>, ISoftDeletable
  where TId : struct, IEquatable<TId>
{
  public bool IsDeleted { get; protected set; }

  public DateTimeOffset? DeletedAt { get; protected set; }

  public string? DeletedBy { get; protected set; }

  public void SoftDelete(string deletedBy, DateTimeOffset? deletedAt = null)
  {
    if (IsDeleted)
    {
      return;
    }

    if (string.IsNullOrWhiteSpace(deletedBy))
    {
      throw new ArgumentException("Deleted by is required.", nameof(deletedBy));
    }

    IsDeleted = true;
    DeletedBy = deletedBy;
    DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
    MarkAsModified(deletedBy, DeletedAt);
  }

  public void Restore(string modifiedBy)
  {
    if (!IsDeleted)
    {
      return;
    }

    IsDeleted = false;
    DeletedBy = null;
    DeletedAt = null;
    MarkAsModified(modifiedBy);
  }
}
