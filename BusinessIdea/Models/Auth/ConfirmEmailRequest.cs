using System.ComponentModel.DataAnnotations;

namespace BusinessIdea.Web.Models.Auth;

/// <summary>Second registration step: the code the user received by email.</summary>
public record ConfirmEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string Code { get; init; } = string.Empty;
}
