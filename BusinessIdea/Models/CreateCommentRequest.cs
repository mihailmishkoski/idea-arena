using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Web.Models;

/// <summary>
/// Body for creating a comment; the post id comes from the route. When
/// <see cref="ParentCommentId"/> is set the comment is a reply and inherits the
/// parent's metric (so <see cref="TargetMetric"/> is ignored).
/// </summary>
public record CreateCommentRequest(
    string Content,
    IdeaMetric TargetMetric = IdeaMetric.General,
    Guid? ParentCommentId = null);
