using BusinessIdea.Domain.Enums;
using MediatR;

namespace BusinessIdea.Application.Features.Comments.Commands.CreateComment;

/// <summary>
/// Adds a comment to an idea. The comment either targets the idea in general or
/// a specific metric the author described (e.g. the exit strategy).
/// </summary>
public record CreateCommentCommand : IRequest<Guid>
{
    public Guid PostId { get; init; }
    public string Content { get; init; } = string.Empty;
    public IdeaMetric TargetMetric { get; init; } = IdeaMetric.General;

    /// <summary>
    /// When set, this comment is a reply. It inherits the parent's target metric
    /// and post, so <see cref="TargetMetric"/> is ignored.
    /// </summary>
    public Guid? ParentCommentId { get; init; }
}
