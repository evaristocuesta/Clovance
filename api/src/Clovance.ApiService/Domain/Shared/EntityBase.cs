namespace Clovance.ApiService.Domain.Shared;

public abstract class EntityBase<TId>
  where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected set; }
}
