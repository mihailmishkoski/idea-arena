namespace BusinessIdea.Application.Common.Models;

/// <summary>
/// Builds absolute links to the SPA for use in emails. Registered as a
/// singleton with the base URL taken from configuration.
/// </summary>
public class PublicAppUrls
{
    public string BaseUrl { get; init; } = "http://localhost:4200";

    public string Idea(Guid postId) => $"{BaseUrl.TrimEnd('/')}/ideas/{postId}";

    public string Messages() => $"{BaseUrl.TrimEnd('/')}/messages";
}
