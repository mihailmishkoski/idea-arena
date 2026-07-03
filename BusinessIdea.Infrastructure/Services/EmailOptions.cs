namespace BusinessIdea.Infrastructure.Services;

/// <summary>SMTP settings; defaults target a local smtp4dev instance.</summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool UseSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@idea-arena.local";
    public string FromName { get; set; } = "Idea Arena";
}
