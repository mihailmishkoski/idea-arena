namespace BusinessIdea.Domain.Common;

/// <summary>
/// Marks an entity that tracks creation / modification timestamps. The values
/// are populated centrally by the persistence layer so handlers never have to
/// remember to set them.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; set; }
    DateTimeOffset? UpdatedAtUtc { get; set; }
}
