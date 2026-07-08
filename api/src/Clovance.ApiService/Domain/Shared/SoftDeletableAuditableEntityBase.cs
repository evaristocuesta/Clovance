namespace Clovance.ApiService.Domain.Shared;

public abstract class SoftDeletableAuditableEntityBase<TId> : AuditableEntityBase<TId>, ISoftDeletable
  where TId : struct, IEquatable<TId>
{
    public bool IsDeleted { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public Guid? DeletedBy { get; protected set; }

    public void SoftDelete(Guid deletedBy, DateTimeOffset? deletedAt = null)
    {
        if (IsDeleted)
        {
            return;
        }

        if (deletedBy.Equals(default(Guid)))
        {
            throw new ArgumentException("Deleted by is required.", nameof(deletedBy));
        }

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
        MarkAsModified(deletedBy, DeletedAt);
    }

    public void Restore(Guid modifiedBy)
    {
        if (!IsDeleted)
        {
            return;
        }

        if (modifiedBy.Equals(default(Guid)))
        {
            throw new ArgumentException("Modifier is required.", nameof(modifiedBy));
        }

        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = null;
        MarkAsModified(modifiedBy);
    }
}
