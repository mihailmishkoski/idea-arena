using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// A single user's vote on a business idea. A unique index on
/// (<see cref="PostId"/>, <see cref="UserId"/>) guarantees at most one vote per
/// user per post.
/// </summary>
public class PostVote : BaseEntity, IAuditableEntity
{
    public Guid PostId { get; set; }
    public BusinessIdeaPost Post { get; set; } = null!;

    /// <summary>Identity id of the voter.</summary>
    public string UserId { get; set; } = string.Empty;

    public VoteDirection Direction { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
