using System.Text;
using BusinessIdea.Application.Common.Exceptions;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Outbox;
using BusinessIdea.Application.Features.Notifications;
using BusinessIdea.Domain.Common;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = BusinessIdea.Application.Common.Exceptions.ValidationException;

namespace BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;

public class ApplyToCofoundCommandHandler : IRequestHandler<ApplyToCofoundCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public ApplyToCofoundCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IRealtimeNotifier notifier)
    {
        _context = context;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<Guid> Handle(ApplyToCofoundCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be signed in to apply.");

        var post = await _context.BusinessIdeas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken)
            ?? throw new NotFoundException(nameof(BusinessIdeaPost), request.PostId);

        if (post.AuthorId == userId)
        {
            throw Invalid("You cannot apply to co-found your own idea.");
        }

        // Applying is deliberately not a repeatable one-click action: one open
        // conversation per pair of users.
        var existing = await _context.Conversations
            .Where(c => c.Status != ChatRequestStatus.Declined)
            .Where(c =>
                (c.RequesterId == userId && c.RecipientId == post.AuthorId) ||
                (c.RequesterId == post.AuthorId && c.RecipientId == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is { Status: ChatRequestStatus.Pending })
        {
            throw Invalid("You already have a pending request with this author. Wait for their response.");
        }
        if (existing is { Status: ChatRequestStatus.Accepted })
        {
            // No repeat emails through re-applying: the channel is already open.
            throw Invalid("You already have an open conversation with this author — message them directly.");
        }

        // Server-side rate limit: initiating conversations always emails a
        // real person, so no account gets to fire these like a machine gun.
        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var initiatedToday = await _context.Conversations
            .CountAsync(c => c.RequesterId == userId && c.CreatedAtUtc > since, cancellationToken);
        if (initiatedToday >= ChatRules.MaxInitiationsPerDay)
        {
            throw Invalid($"Daily limit reached ({ChatRules.MaxInitiationsPerDay} requests per 24h). Try again tomorrow.");
        }

        var conversation = new Conversation
        {
            RequesterId = userId,
            RecipientId = post.AuthorId,
            PostId = request.PostId,
        };
        _context.Conversations.Add(conversation);

        var applicantName = await _context.Authors
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? "Someone";

        var notification = new Notification
        {
            UserId = post.AuthorId,
            Type = NotificationType.ChatRequest,
            Text = $"{applicantName} applied to co-found “{post.Name}”.",
            TargetId = conversation.Id,
        };
        _context.Notifications.Add(notification);

        // The application details travel ONLY by email: in-app the author gets
        // a plain request they can accept or decline.
        _context.OutboxMessages.Add(OutboxMessageFactory.Create(
            OutboxEventTypes.CofounderApplied,
            new CofounderAppliedPayload(conversation.Id, BuildApplicationText(post.Name, request))));

        await _context.SaveChangesAsync(cancellationToken);

        await _notifier.SendToUserAsync(post.AuthorId, "notification", new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Text = notification.Text,
            TargetId = notification.TargetId,
            IsRead = false,
            CreatedAtUtc = notification.CreatedAtUtc,
        }, cancellationToken);

        return conversation.Id;
    }

    /// <summary>Formats the optional fields into the first chat message.</summary>
    internal static string BuildApplicationText(string postName, ApplyToCofoundCommand request)
    {
        var builder = new StringBuilder();
        builder.Append("Co-founder application — “").Append(postName).Append('”');

        AppendField(builder, "Role I can take", request.Role);
        AppendField(builder, "Skills & experience", request.Skills);
        AppendField(builder, "Why this idea", request.Motivation);
        AppendField(builder, "Availability", request.Availability);
        AppendField(builder, "Links", request.ContactLink);

        if (builder.Length <= postName.Length + 40)
        {
            builder.Append("\n\nI'd love to help build this — let's talk!");
        }

        return builder.ToString();
    }

    private static void AppendField(StringBuilder builder, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.Append("\n\n").Append(label).Append(": ").Append(value.Trim());
        }
    }

    private static ValidationException Invalid(string message) => new(new[]
    {
        new FluentValidation.Results.ValidationFailure("PostId", message),
    });
}
