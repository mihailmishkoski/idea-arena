using BusinessIdea.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BusinessIdea.Infrastructure.Identity;

/// <summary>
/// Finds (or lazily creates) the system "AI Critic" account that authors the
/// seeded questions. It has no password, so nobody can log in as it.
/// </summary>
public class AiCriticUserProvider : IAiCriticUserProvider
{
    public const string Email = "ai-critic@idea-arena.local";

    private readonly UserManager<ApplicationUser> _userManager;

    public AiCriticUserProvider(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GetAiCriticUserIdAsync(CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(Email);
        if (existing is not null)
        {
            return existing.Id;
        }

        var critic = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true,
            DisplayName = "AI Critic",
            AvatarId = "robot",
        };

        var result = await _userManager.CreateAsync(critic);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Could not create the AI Critic account: " +
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return critic.Id;
    }
}
