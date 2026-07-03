using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.Comments.Queries;

/// <summary>Projection of a comment with its vote tallies for display.</summary>
public class CommentDto
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public string? AuthorAvatar { get; init; }
    public string Content { get; init; } = string.Empty;
    public IdeaMetric TargetMetric { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }

    public int UpVotes { get; init; }
    public int DownVotes { get; init; }
    public int Score => UpVotes - DownVotes;

    public VoteDirection? CurrentUserVote { get; init; }
}
