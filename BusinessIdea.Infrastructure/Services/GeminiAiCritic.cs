using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;
using BusinessIdea.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessIdea.Infrastructure.Services;

/// <summary>
/// <see cref="IAiCritic"/> adapter for the Google Gemini API (free tier).
/// Asks for strict JSON output and maps each question onto the pitch metric it
/// challenges. Swapping providers means replacing this one class.
/// </summary>
public class GeminiAiCritic : IAiCritic
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAiCritic> _logger;

    public GeminiAiCritic(HttpClient http, IOptions<GeminiOptions> options, ILogger<GeminiAiCritic> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AiCriticQuestion>> GenerateQuestionsAsync(
        BusinessIdeaPost idea, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured. Set it with: " +
                "dotnet user-secrets set \"Gemini:ApiKey\" \"<key>\" (project BusinessIdea).");
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            contents = new[] { new { parts = new[] { new { text = BuildPrompt(idea) } } } },
            generationConfig = new { responseMimeType = "application/json" },
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini returned {(int)response.StatusCode}: {Truncate(raw, 400)}");
        }

        var text = ExtractText(raw)
            ?? throw new InvalidOperationException("Gemini response contained no text candidate.");

        return ParseQuestions(text);
    }

    private string BuildPrompt(BusinessIdeaPost idea)
    {
        var metricNames = string.Join(", ", Enum.GetNames<IdeaMetric>().Where(n => n != nameof(IdeaMetric.General)));

        return $$"""
            You are a sharp but fair startup investor reviewing a pitch on a community platform.
            Write the {{_options.QuestionCount}} toughest questions this founder must be able to answer.
            Aim each question at the WEAKEST parts of the pitch. Be specific to this idea, not generic.
            Keep each question under 50 words and address the founder directly ("you", "your").

            Return ONLY a JSON array in this exact shape:
            [{"metric": "<one of: {{metricNames}}>", "question": "<the question>"}]

            The pitch:
            Name: {{idea.Name}}
            Unique value proposition: {{idea.UniqueValueProposition}}
            Problem: {{idea.Problem}}
            Solution: {{idea.Solution}}
            Competition: {{idea.Competition ?? "(not provided)"}}
            Income strategy: {{idea.IncomeStrategy ?? "(not provided)"}}
            Exit strategy: {{idea.ExitStrategy ?? "(not provided)"}}
            """;
    }

    private static string? ExtractText(string rawResponse)
    {
        using var doc = JsonDocument.Parse(rawResponse);
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.GetArrayLength() == 0)
        {
            return null;
        }

        var parts = candidates[0].GetProperty("content").GetProperty("parts");
        return parts.GetArrayLength() > 0 ? parts[0].GetProperty("text").GetString() : null;
    }

    private IReadOnlyList<AiCriticQuestion> ParseQuestions(string json)
    {
        var items = JsonSerializer.Deserialize<List<QuestionItem>>(json, JsonOptions) ?? [];

        var questions = items
            .Where(i => !string.IsNullOrWhiteSpace(i.Question))
            .Select(i => new AiCriticQuestion(
                Enum.TryParse<IdeaMetric>(i.Metric, ignoreCase: true, out var metric)
                    ? metric
                    : IdeaMetric.General,
                i.Question!.Trim()))
            .Take(_options.QuestionCount)
            .ToList();

        if (questions.Count == 0)
        {
            _logger.LogWarning("Gemini answered but no questions could be parsed: {Json}", Truncate(json, 400));
        }

        return questions;
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";

    private sealed class QuestionItem
    {
        [JsonPropertyName("metric")]
        public string? Metric { get; set; }

        [JsonPropertyName("question")]
        public string? Question { get; set; }
    }
}
