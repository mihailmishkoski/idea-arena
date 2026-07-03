namespace BusinessIdea.Domain.Common;

/// <summary>
/// Base type for all persisted entities. Carries the surrogate key so that
/// individual entities only have to describe what makes them unique.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
