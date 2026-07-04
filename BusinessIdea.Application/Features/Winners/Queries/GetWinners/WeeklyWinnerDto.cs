namespace BusinessIdea.Application.Features.Winners.Queries.GetWinners;

/// <summary>One hall-of-fame entry: the winning idea of a competition week.</summary>
public class WeeklyWinnerDto
{
    public Guid Id { get; init; }
    public DateTimeOffset PeriodStartUtc { get; init; }
    public DateTimeOffset PeriodEndUtc { get; init; }

    /// <summary>Null when the winning post was deleted; the snapshot fields still describe it.</summary>
    public Guid? PostId { get; init; }
    public string PostName { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public string? AuthorAvatar { get; init; }

    public int UpVotes { get; init; }
    public int DownVotes { get; init; }
    public int Score { get; init; }
    public int CommentCount { get; init; }
}
