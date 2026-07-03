using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyCollection<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<NotificationDto>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in.");

        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(Math.Clamp(request.Limit, 1, 100))
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Text = n.Text,
                TargetId = n.TargetId,
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }
}
