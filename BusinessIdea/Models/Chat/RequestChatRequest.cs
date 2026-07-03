namespace BusinessIdea.Web.Models.Chat;

/// <summary>Body for starting a chat with another user.</summary>
public record RequestChatRequest(string RecipientId, Guid? PostId);
