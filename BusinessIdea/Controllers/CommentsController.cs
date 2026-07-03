using BusinessIdea.Application.Features.Comments.Commands.CreateComment;
using BusinessIdea.Application.Features.Comments.Commands.DeleteComment;
using BusinessIdea.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Authorize]
public class CommentsController : ApiControllerBase
{
    /// <summary>Add a comment to an idea, optionally anchored to a metric.</summary>
    [HttpPost("/api/ideas/{postId:guid}/comments")]
    public async Task<ActionResult<Guid>> Create(
        Guid postId, CreateCommentRequest request, CancellationToken ct)
    {
        var command = new CreateCommentCommand
        {
            PostId = postId,
            Content = request.Content,
            TargetMetric = request.TargetMetric,
            ParentCommentId = request.ParentCommentId
        };

        var id = await Mediator.Send(command, ct);
        return Ok(id);
    }

    /// <summary>Delete one of your comments.</summary>
    [HttpDelete("/api/comments/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCommentCommand(id), ct);
        return NoContent();
    }
}
