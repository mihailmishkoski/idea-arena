using System.ComponentModel.DataAnnotations;

namespace BusinessIdea.Web.Models.Auth;

/// <summary>Payload for SPA login.</summary>
public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}
