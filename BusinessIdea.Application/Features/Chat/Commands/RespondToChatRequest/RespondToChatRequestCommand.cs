using MediatR;

namespace BusinessIdea.Application.Features.Chat.Commands.RespondToChatRequest;

/// <summary>Accepts or declines a pending chat request. Only the recipient may respond.</summary>
public record RespondToChatRequestCommand(Guid ConversationId, bool Accept) : IRequest;
