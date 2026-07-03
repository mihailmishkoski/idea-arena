namespace BusinessIdea.Web.Models.Chat;

/// <summary>Body for a co-founder application. All pitch fields are optional.</summary>
public record ApplyCofoundRequest(
    Guid PostId,
    string? Role,
    string? Skills,
    string? Motivation,
    string? Availability,
    string? ContactLink);
