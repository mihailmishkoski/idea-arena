using MediatR;

namespace BusinessIdea.Application.Features.Chat.Queries.GetConversationMessages;

/// <summary>
/// The messages of a conversation the current user participates in, oldest
/// first. Fetching also marks the other side's messages as read.
/// </summary>
public record GetConversationMessagesQuery(Guid ConversationId)
    : IRequest<IReadOnlyCollection<ChatMessageDto>>;
