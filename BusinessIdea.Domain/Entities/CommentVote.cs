using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// A single user's vote on a comment. A unique index on
/// (<see cref="CommentId"/>, <see cref="UserId"/>) guarantees at most one vote
/// per user per comment.
/// </summary>
public class CommentVote : BaseEntity, IAuditableEntity
{
    public Guid CommentId { get; set; }
    public Comment Comment { get; set; } = null!;

    /// <summary>Identity id of the voter.</summary>
    public string UserId { get; set; } = string.Empty;

    public VoteDirection Direction { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
