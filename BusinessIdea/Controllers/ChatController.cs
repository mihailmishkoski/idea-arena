using BusinessIdea.Application.Features.Chat;
using BusinessIdea.Application.Features.Chat.Commands.ApplyToCofound;
using BusinessIdea.Application.Features.Chat.Commands.RequestChat;
using BusinessIdea.Application.Features.Chat.Commands.RespondToChatRequest;
using BusinessIdea.Application.Features.Chat.Commands.SendChatMessage;
using BusinessIdea.Application.Features.Chat.Queries.GetConversationMessages;
using BusinessIdea.Application.Features.Chat.Queries.GetConversations;
using BusinessIdea.Web.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

[Authorize]
[Route("api/chat")]
public class ChatController : ApiControllerBase
{
    /// <summary>Ask a user to chat. Returns the (new or existing) conversation id.</summary>
    [HttpPost("requests")]
    public async Task<ActionResult<Guid>> RequestChat(RequestChatRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new RequestChatCommand(request.RecipientId, request.PostId), ct));

    /// <summary>
    /// Apply to co-found an idea: creates the request, delivers the structured
    /// application as the first message, and notifies the author in-app + by email.
    /// </summary>
    [HttpPost("cofound")]
    public async Task<ActionResult<Guid>> ApplyToCofound(ApplyCofoundRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new ApplyToCofoundCommand(
            request.PostId, request.Role, request.Skills,
            request.Motivation, request.Availability, request.ContactLink), ct));

    /// <summary>Accept or decline a pending chat request (recipient only).</summary>
    [HttpPost("requests/{conversationId:guid}/respond")]
    public async Task<IActionResult> Respond(
        Guid conversationId, RespondChatRequest request, CancellationToken ct)
    {
        await Mediator.Send(new RespondToChatRequestCommand(conversationId, request.Accept), ct);
        return NoContent();
    }

    /// <summary>All conversations involving the current user.</summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyCollection<ConversationDto>>> GetConversations(CancellationToken ct)
        => Ok(await Mediator.Send(new GetConversationsQuery(), ct));

    /// <summary>The messages of a conversation (marks incoming ones as read).</summary>
    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyCollection<ChatMessageDto>>> GetMessages(
        Guid conversationId, CancellationToken ct)
        => Ok(await Mediator.Send(new GetConversationMessagesQuery(conversationId), ct));

    /// <summary>Send a message; it is also pushed to the other user over the WebSocket.</summary>
    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        Guid conversationId, SendMessageRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new SendChatMessageCommand(conversationId, request.Content), ct));
}
