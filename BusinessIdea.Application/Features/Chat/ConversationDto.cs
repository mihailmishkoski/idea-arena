using BusinessIdea.Domain.Enums;

namespace BusinessIdea.Application.Features.Chat;

/// <summary>A conversation as seen by the current user ("the other side" resolved).</summary>
public class ConversationDto
{
    public Guid Id { get; init; }
    public ChatRequestStatus Status { get; init; }

    /// <summary>True when the current user initiated the request.</summary>
    public bool IAmRequester { get; init; }

    public string OtherUserId { get; init; } = string.Empty;
    public string? OtherUserName { get; init; }
    public string? OtherUserAvatar { get; init; }

    /// <summary>The idea the chat started from, if any.</summary>
    public Guid? PostId { get; init; }
    public string? PostName { get; init; }

    public string? LastMessage { get; init; }
    public DateTimeOffset? LastMessageAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Messages from the other user the current user hasn't seen.</summary>
    public int UnreadCount { get; init; }
}
