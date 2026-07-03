using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.Votes;

/// <summary>
/// Returned after a vote is cast so the client can update the UI without an
/// extra round trip: the fresh tallies plus the caller's resulting vote state.
/// </summary>
public class VoteResultDto
{
    public int UpVotes { get; init; }
    public int DownVotes { get; init; }
    public int Score => UpVotes - DownVotes;

    /// <summary>The caller's vote after the operation, or null if it was cleared.</summary>
    public VoteDirection? CurrentUserVote { get; init; }
}
