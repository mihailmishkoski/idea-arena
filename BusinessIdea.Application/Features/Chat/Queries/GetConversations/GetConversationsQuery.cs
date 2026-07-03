using MediatR;

namespace BusinessIdea.Application.Features.Chat.Queries.GetConversations;

/// <summary>All conversations (and pending requests) involving the current user.</summary>
public record GetConversationsQuery : IRequest<IReadOnlyCollection<ConversationDto>>;
