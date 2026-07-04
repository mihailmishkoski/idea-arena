using BusinessIdea.Application.Common.Models;
using BusinessIdea.Application.Features.Winners.Queries.GetWinners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Route("api/winners")]
public class WinnersController : ApiControllerBase
{
    /// <summary>Browse the hall of fame: past weekly winners, newest first.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<WeeklyWinnerDto>>> Get(
        [FromQuery] GetWinnersQuery query, CancellationToken ct)
        => Ok(await Mediator.Send(query, ct));
}
