using System.ComponentModel.DataAnnotations;

namespace BusinessIdea.Web.Models.Auth;

/// <summary>Payload for SPA registration.</summary>
public record RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    /// <summary>Key of the chosen avatar from the built-in collection.</summary>
    [StringLength(32)]
    public string? AvatarId { get; init; }
}
