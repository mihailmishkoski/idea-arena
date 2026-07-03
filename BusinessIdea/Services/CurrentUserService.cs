using System.Security.Claims;
using BusinessIdea.Application.Common.Interfaces;

namespace BusinessIdea.Web.Services;

/// <summary>
/// Web-layer implementation of <see cref="ICurrentUserService"/>. It reads the
/// authenticated user from the ambient <see cref="HttpContext"/>, keeping all
/// HTTP concerns out of the Application layer.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
