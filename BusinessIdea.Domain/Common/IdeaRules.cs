namespace BusinessIdea.Domain.Common;

/// <summary>
/// Business rules for the idea "competition". An idea is open for voting for a
/// fixed window after it is posted; once that window closes it leaves the active
/// feed and its final score decides the winner.
/// </summary>
public static class IdeaRules
{
    /// <summary>How long an idea stays active in the feed after being posted.</summary>
    public const int LifetimeDays = 14;
}
