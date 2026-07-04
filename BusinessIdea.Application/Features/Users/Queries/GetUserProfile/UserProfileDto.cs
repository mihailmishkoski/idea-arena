using BusinessIdea.Application.Features.BusinessIdeas.Queries;

namespace BusinessIdea.Application.Features.Users.Queries.GetUserProfile;

/// <summary>A member's public profile: identity basics plus community stats.</summary>
public class UserProfileDto
{
    public string Id { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? AvatarId { get; init; }

    public int IdeasCount { get; init; }
    public int CommentsCount { get; init; }

    /// <summary>Net votes received across the member's ideas and comments.</summary>
    public int Karma { get; init; }

    /// <summary>How many competition weeks this member has won.</summary>
    public int Wins { get; init; }

    /// <summary>The member's most recent ideas (active and closed alike).</summary>
    public IReadOnlyCollection<BusinessIdeaSummaryDto> RecentIdeas { get; init; } =
        Array.Empty<BusinessIdeaSummaryDto>();
}
