using BusinessIdea.Domain.Common;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// The winning idea of one competition week: among all ideas whose
/// <see cref="IdeaRules.LifetimeDays"/>-day window closed during that week, the
/// one with the highest final score. Key facts (name, author, tallies) are
/// snapshotted at declaration time so the hall of fame survives even if the
/// post itself is later deleted.
/// </summary>
public class WeeklyWinner : BaseEntity, IAuditableEntity
{
    /// <summary>Monday 00:00 UTC of the week the competition closed in.</summary>
    public DateTimeOffset PeriodStartUtc { get; set; }

    /// <summary>Exclusive end: the following Monday 00:00 UTC.</summary>
    public DateTimeOffset PeriodEndUtc { get; set; }

    /// <summary>Null once the winning post has been deleted by its author.</summary>
    public Guid? PostId { get; set; }
    public BusinessIdeaPost? Post { get; set; }

    public string PostName { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;

    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public int CommentCount { get; set; }
    public int Score { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
