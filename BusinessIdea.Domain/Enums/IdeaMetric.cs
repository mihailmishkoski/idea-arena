namespace BusinessIdea.Domain.Enums;

/// <summary>
/// Identifies which part of a business idea a comment is attached to. A comment
/// can either target the whole idea (<see cref="General"/>) or one specific
/// metric the author described, enabling focused discussion threads.
/// </summary>
public enum IdeaMetric
{
    General = 0,
    UniqueValueProposition = 1,
    Problem = 2,
    Solution = 3,
    Competition = 4,
    IncomeStrategy = 5,
    ExitStrategy = 6,
    VideoPitch = 7
}
