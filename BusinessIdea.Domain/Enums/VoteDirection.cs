namespace BusinessIdea.Domain.Enums;

/// <summary>
/// The direction of a cast vote. The backing integer values are meaningful:
/// summing them across all votes yields the net score of a post or comment.
/// </summary>
public enum VoteDirection
{
    Down = -1,
    Up = 1
}
