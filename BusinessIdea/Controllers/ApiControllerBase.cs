using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

/// <summary>
/// Base for API controllers. Exposes MediatR's <see cref="ISender"/> so
/// controllers stay thin: they translate HTTP to a command/query, send it, and
/// translate the result back to HTTP — no business logic lives here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
