using System.ComponentModel.DataAnnotations;

namespace BusinessIdea.Web.Models.Auth;

/// <summary>Asks for a fresh verification code for a not-yet-confirmed account.</summary>
public record ResendCodeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}
