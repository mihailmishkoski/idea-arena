using BusinessIdea.Domain.Common;

namespace BusinessIdea.Domain.Entities;

/// <summary>
/// A business idea shared by a user. This is the primary aggregate: it owns the
/// votes cast on it and the comments discussing it.
/// </summary>
public class BusinessIdeaPost : BaseEntity, IAuditableEntity
{
    /// <summary>Name of the business. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>What makes this idea uniquely valuable. Required.</summary>
    public string UniqueValueProposition { get; set; } = string.Empty;

    /// <summary>The problem the business solves. Required.</summary>
    public string Problem { get; set; } = string.Empty;

    /// <summary>How the business solves the problem. Required.</summary>
    public string Solution { get; set; } = string.Empty;

    /// <summary>Competitive landscape. Optional.</summary>
    public string? Competition { get; set; }

    /// <summary>How the business intends to make money. Optional.</summary>
    public string? IncomeStrategy { get; set; }

    /// <summary>Planned exit strategy. Optional.</summary>
    public string? ExitStrategy { get; set; }

    /// <summary>Link to a video pitch. Optional.</summary>
    public string? VideoPitchUrl { get; set; }

    /// <summary>Identity id of the user who authored the idea.</summary>
    public string AuthorId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<PostVote> Votes { get; set; } = new List<PostVote>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
