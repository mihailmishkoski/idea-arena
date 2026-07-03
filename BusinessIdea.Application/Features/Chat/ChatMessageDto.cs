namespace BusinessIdea.Application.Features.Chat;

public class ChatMessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public string SenderId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; init; }
}
