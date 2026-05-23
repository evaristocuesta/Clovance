namespace Clovance.ApiService.Domain.Shared;

public interface ISoftDeletable
{
  bool IsDeleted { get; }

  DateTimeOffset? DeletedAt { get; }

  string? DeletedBy { get; }
}
