using BusinessIdea.Application.Features.Users.Queries.GetUserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    /// <summary>A member's public profile: stats plus their recent ideas.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetUserProfileQuery(id), ct));
}
