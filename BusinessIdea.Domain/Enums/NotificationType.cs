namespace BusinessIdea.Domain.Enums;

/// <summary>What kind of event a notification describes.</summary>
public enum NotificationType
{
    /// <summary>Someone commented on your idea.</summary>
    PostComment = 0,

    /// <summary>Someone replied to your comment.</summary>
    CommentReply = 1,

    /// <summary>Someone wants to chat with you.</summary>
    ChatRequest = 2,

    /// <summary>Your chat request was accepted.</summary>
    ChatAccepted = 3,

    /// <summary>A new chat message arrived.</summary>
    NewMessage = 4
}
