using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries;

/// <summary>Lightweight projection of an idea for list / feed views.</summary>
public class BusinessIdeaSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UniqueValueProposition { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public string? AuthorAvatar { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }

    public int UpVotes { get; init; }
    public int DownVotes { get; init; }
    public int Score => UpVotes - DownVotes;
    public int CommentCount { get; init; }

    /// <summary>The current user's vote on this idea, or null if they haven't voted.</summary>
    public VoteDirection? CurrentUserVote { get; init; }
}
