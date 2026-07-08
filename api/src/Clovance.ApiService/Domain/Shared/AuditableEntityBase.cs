namespace Clovance.ApiService.Domain.Shared;

public abstract class AuditableEntityBase<TId> : EntityBase<TId>
  where TId : struct, IEquatable<TId>
{
    public DateTimeOffset CreatedAt { get; protected set; }

    public Guid? CreatedBy { get; protected set; } = default;

    public DateTimeOffset? ModifiedAt { get; protected set; }

    public Guid? ModifiedBy { get; protected set; } = default;

    protected void MarkAsCreated(Guid createdBy, DateTimeOffset? createdAt = null)
    {
        if (createdBy.Equals(default(Guid)))
        {
            throw new ArgumentException("Creator is required.", nameof(createdBy));
        }

        CreatedBy = createdBy;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    protected void MarkAsModified(Guid modifiedBy, DateTimeOffset? modifiedAt = null)
    {
        if (modifiedBy.Equals(default(Guid)))
        {
            throw new ArgumentException("Modifier is required.", nameof(modifiedBy));
        }

        ModifiedBy = modifiedBy;
        ModifiedAt = modifiedAt ?? DateTimeOffset.UtcNow;
    }
}
