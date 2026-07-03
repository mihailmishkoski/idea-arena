using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Features.Notifications;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessIdea.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public CreateCommentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IRealtimeNotifier notifier)
    {
        _context = context;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to comment.");

        Guid postId;
        IdeaMetric metric;
        Guid? parentId = null;
        string? notifyUserId;
        NotificationType notificationType;

        if (request.ParentCommentId is { } parentCommentId)
        {
            // A reply inherits the parent's post and metric — and notifies its author.
            var parent = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == parentCommentId, cancellationToken)
                ?? throw new NotFoundException(nameof(Comment), parentCommentId);

            postId = parent.PostId;
            metric = parent.TargetMetric;
            parentId = parent.Id;
            notifyUserId = parent.AuthorId;
            notificationType = NotificationType.CommentReply;
        }
        else
        {
            var postAuthorId = await _context.BusinessIdeas
                .Where(i => i.Id == request.PostId)
                .Select(i => i.AuthorId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(BusinessIdeaPost), request.PostId);

            postId = request.PostId;
            metric = request.TargetMetric;
            notifyUserId = postAuthorId;
            notificationType = NotificationType.PostComment;
        }

        var comment = new Comment
        {
            PostId = postId,
            AuthorId = userId,
            Content = request.Content.Trim(),
            TargetMetric = metric,
            ParentCommentId = parentId
        };

        _context.Comments.Add(comment);

        // Never notify yourself about your own comment.
        Notification? notification = null;
        if (notifyUserId != userId)
        {
            var commenterName = await _context.Authors
                .Where(u => u.Id == userId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(cancellationToken) ?? "Someone";

            var postName = await _context.BusinessIdeas
                .Where(i => i.Id == postId)
                .Select(i => i.Name)
                .FirstAsync(cancellationToken);

            notification = new Notification
            {
                UserId = notifyUserId,
                Type = notificationType,
                Text = notificationType == NotificationType.CommentReply
                    ? $"{commenterName} replied to your comment on “{postName}”."
                    : $"{commenterName} commented on your idea “{postName}”.",
                TargetId = postId,
            };
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (notification is not null)
        {
            await _notifier.SendToUserAsync(notification.UserId, "notification", new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Text = notification.Text,
                TargetId = notification.TargetId,
                IsRead = false,
                CreatedAtUtc = notification.CreatedAtUtc,
            }, cancellationToken);
        }

        return comment.Id;
    }
}
