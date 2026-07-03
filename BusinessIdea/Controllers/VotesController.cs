using BusinessIdea.Application.Features.Votes;
using BusinessIdea.Application.Features.Votes.CastCommentVote;
using BusinessIdea.Application.Features.Votes.CastPostVote;
using BusinessIdea.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Authorize]
public class VotesController : ApiControllerBase
{
    /// <summary>
    /// Vote on an idea. Sending the same direction again clears your vote;
    /// the opposite direction flips it. Returns the fresh tallies.
    /// </summary>
    [HttpPost("/api/ideas/{postId:guid}/vote")]
    public async Task<ActionResult<VoteResultDto>> VoteOnIdea(
        Guid postId, VoteRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new CastPostVoteCommand(postId, request.Direction), ct));

    /// <summary>Vote on a comment. Same toggle semantics as voting on an idea.</summary>
    [HttpPost("/api/comments/{commentId:guid}/vote")]
    public async Task<ActionResult<VoteResultDto>> VoteOnComment(
        Guid commentId, VoteRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new CastCommentVoteCommand(commentId, request.Direction), ct));
}
