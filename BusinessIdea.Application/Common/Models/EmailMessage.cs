namespace BusinessIdea.Application.Common.Models;

/// <summary>A single outgoing email, transport-agnostic.</summary>
public record EmailMessage(string ToEmail, string? ToName, string Subject, string HtmlBody);
