using MediatR;

namespace BusinessIdea.Application.Features.Chat.Commands.RequestChat;

/// <summary>
/// Asks another user (usually an idea's author) to chat. If a non-declined
/// conversation between the two users already exists, its id is returned
/// instead of creating a duplicate.
/// </summary>
public record RequestChatCommand(string RecipientId, Guid? PostId) : IRequest<Guid>;
