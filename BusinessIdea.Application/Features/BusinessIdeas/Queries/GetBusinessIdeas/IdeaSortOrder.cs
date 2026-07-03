namespace BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;

/// <summary>How a feed of ideas should be ordered (Reddit-style options).</summary>
public enum IdeaSortOrder
{
    /// <summary>Highest net score first (Reddit "top").</summary>
    Top = 0,

    /// <summary>Most recently posted first (Reddit "new").</summary>
    New = 1,

    /// <summary>Most engagement (votes + comments) first (Reddit "best/hot").</summary>
    Best = 2,

    /// <summary>Most divisive first — lots of downvotes and overall activity.</summary>
    Controversial = 3,

    /// <summary>Oldest active idea first.</summary>
    Old = 4,

    /// <summary>Closed (expired) ideas ranked by final score — the winners.</summary>
    Winners = 5,
}
