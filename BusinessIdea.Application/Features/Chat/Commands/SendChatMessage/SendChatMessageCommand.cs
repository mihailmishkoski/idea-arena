using MediatR;

namespace BusinessIdea.Application.Features.Chat.Commands.SendChatMessage;

/// <summary>
/// Sends a message in an accepted conversation the current user participates in.
/// Returns the stored message; the other participant also receives it in real
/// time over the WebSocket.
/// </summary>
public record SendChatMessageCommand(Guid ConversationId, string Content) : IRequest<ChatMessageDto>;
