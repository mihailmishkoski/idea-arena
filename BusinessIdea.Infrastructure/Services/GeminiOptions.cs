namespace BusinessIdea.Infrastructure.Services;

/// <summary>Google Gemini settings. The key lives in user-secrets, never in the repo.</summary>
public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public int QuestionCount { get; set; } = 3;
}
