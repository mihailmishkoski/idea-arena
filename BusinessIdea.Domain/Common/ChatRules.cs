namespace BusinessIdea.Domain.Common;

/// <summary>
/// Abuse limits for conversation-starting actions. They exist server-side so
/// no client (browser, Postman, script) can flood other users with emails.
/// </summary>
public static class ChatRules
{
    /// <summary>Max new conversations (chat requests + co-founder applications) one account may initiate per 24h.</summary>
    public const int MaxInitiationsPerDay = 10;
}
