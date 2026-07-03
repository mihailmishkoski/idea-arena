using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.CreateBusinessIdea;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.DeleteBusinessIdea;
using BusinessIdea.Application.Features.BusinessIdeas.Commands.UpdateBusinessIdea;
using BusinessIdea.Application.Features.BusinessIdeas.Queries;
using BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeaById;
using BusinessIdea.Application.Features.BusinessIdeas.Queries.GetBusinessIdeas;
using BusinessIdea.Application.Features.Comments.Queries;
using BusinessIdea.Application.Features.Comments.Queries.GetPostComments;
using BusinessIdea.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Route("api/ideas")]
public class BusinessIdeasController : ApiControllerBase
{
    /// <summary>Browse the idea feed (paged, sorted by top score or newest).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<BusinessIdeaSummaryDto>>> Get(
        [FromQuery] GetBusinessIdeasQuery query, CancellationToken ct)
        => Ok(await Mediator.Send(query, ct));

    /// <summary>Get a single idea with its vote tallies.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessIdeaDetailDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetBusinessIdeaByIdQuery(id), ct));

    /// <summary>Get the comments on an idea, optionally filtered to one metric.</summary>
    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<CommentDto>>> GetComments(
        Guid id, [FromQuery] IdeaMetric? metric, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPostCommentsQuery(id, metric), ct));

    /// <summary>Post a new business idea.</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Create(CreateBusinessIdeaCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Update one of your ideas.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateBusinessIdeaCommand command, CancellationToken ct)
    {
        await Mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    /// <summary>Delete one of your ideas.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteBusinessIdeaCommand(id), ct);
        return NoContent();
    }
}
