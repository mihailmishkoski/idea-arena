using BusinessIdea.Application.Common.Models;
using BusinessIdea.Domain.Entities;

namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// LLM port: given a freshly posted idea, produce a few tough investor-style
/// questions, each targeting the pitch metric it challenges. The provider
/// (Gemini, Claude, a local model…) is an Infrastructure adapter — swapping it
/// never touches the Application layer.
/// </summary>
public interface IAiCritic
{
    Task<IReadOnlyList<AiCriticQuestion>> GenerateQuestionsAsync(
        BusinessIdeaPost idea, CancellationToken cancellationToken);
}
