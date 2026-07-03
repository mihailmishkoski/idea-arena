using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// A comment on a business idea. It either discusses the idea in general or is
/// anchored to a specific <see cref="IdeaMetric"/> the author described.
/// Comments carry their own votes to surface the best discussion points.
/// </summary>
public class Comment : BaseEntity, IAuditableEntity
{
    public Guid PostId { get; set; }
    public BusinessIdeaPost Post { get; set; } = null!;

    /// <summary>Identity id of the comment author.</summary>
    public string AuthorId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>Which part of the idea this comment is anchored to.</summary>
    public IdeaMetric TargetMetric { get; set; } = IdeaMetric.General;

    /// <summary>
    /// The comment this one replies to, or null for a top-level comment. Replies
    /// inherit their parent's <see cref="TargetMetric"/>.
    /// </summary>
    public Guid? ParentCommentId { get; set; }
    public Comment? Parent { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
}
