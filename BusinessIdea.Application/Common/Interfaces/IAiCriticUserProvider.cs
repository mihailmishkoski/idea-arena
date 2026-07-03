namespace BusinessIdea.Application.Common.Interfaces;

/// <summary>
/// Supplies the identity id of the system "AI Critic" account that authors the
/// seeded questions. Creating users is an Identity (Infrastructure) concern,
/// so the Application layer only asks for the id.
/// </summary>
public interface IAiCriticUserProvider
{
    Task<string> GetAiCriticUserIdAsync(CancellationToken cancellationToken);
}
