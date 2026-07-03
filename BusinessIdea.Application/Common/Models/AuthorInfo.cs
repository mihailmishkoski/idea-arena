namespace BusinessIdea.Application.Common.Models;

/// <summary>
/// A minimal, Identity-free view of a user. Exposed by
/// <see cref="Interfaces.IApplicationDbContext"/> so read queries can project an
/// author's display name without the Application layer knowing about ASP.NET
/// Identity.
/// </summary>
public class AuthorInfo
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? AvatarId { get; set; }
}
