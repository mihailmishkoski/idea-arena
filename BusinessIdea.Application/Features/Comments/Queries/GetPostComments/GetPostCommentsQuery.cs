using BusinessIdea.Domain.Enums;
using MediatR;

namespace BusinessIdea.Application.Features.Comments.Queries.GetPostComments;

/// <summary>
/// Returns the comments on an idea, best-scored first. Optionally filtered to a
/// single metric so the UI can show a focused discussion (e.g. only comments on
/// the exit strategy).
/// </summary>
public record GetPostCommentsQuery(Guid PostId, IdeaMetric? TargetMetric = null)
    : IRequest<IReadOnlyCollection<CommentDto>>;
