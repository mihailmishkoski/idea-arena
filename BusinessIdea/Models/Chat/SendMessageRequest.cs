namespace BusinessIdea.Web.Models.Chat;

/// <summary>Body for sending a chat message; the conversation id is in the route.</summary>
public record SendMessageRequest(string Content);
