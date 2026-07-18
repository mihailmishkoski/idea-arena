using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.BusinessIdeas.Queries;

/// <summary>Full projection of a single idea for the detail view.</summary>
public class BusinessIdeaDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UniqueValueProposition { get; init; } = string.Empty;
    public string Problem { get; init; } = string.Empty;
    public string Solution { get; init; } = string.Empty;
    public List<BusinessIdeaCategory> Categories { get; init; } = new();
    public string? Competition { get; init; }
    public string? IncomeStrategy { get; init; }
    public string? ExitStrategy { get; init; }
    public string? VideoPitchUrl { get; init; }

    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public string? AuthorAvatar { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? UpdatedAtUtc { get; init; }

    public int UpVotes { get; init; }
    public int DownVotes { get; init; }
    public int Score => UpVotes - DownVotes;
    public int CommentCount { get; init; }

    public VoteDirection? CurrentUserVote { get; init; }
}
