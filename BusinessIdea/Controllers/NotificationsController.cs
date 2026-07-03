using BusinessIdea.Application.Features.Notifications;
using BusinessIdea.Application.Features.Notifications.Commands.MarkNotificationsRead;
using BusinessIdea.Application.Features.Notifications.Queries.GetNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Authorize]
[Route("api/notifications")]
public class NotificationsController : ApiControllerBase
{
    /// <summary>The current user's most recent notifications.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationDto>>> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetNotificationsQuery(), ct));

    /// <summary>Marks all notifications as read (bell dropdown opened).</summary>
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkRead(CancellationToken ct)
    {
        await Mediator.Send(new MarkNotificationsReadCommand(), ct);
        return NoContent();
    }
}
