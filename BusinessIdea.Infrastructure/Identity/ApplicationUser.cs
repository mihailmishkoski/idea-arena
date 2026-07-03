using Microsoft.AspNetCore.Identity;

namespace BusinessIdea.Infrastructure.Identity;

/// <summary>
/// The application's user. Extends the default Identity user so we can add
/// profile fields later (display name, avatar, etc.) without a migration churn
/// on the rest of the schema. Lives in Infrastructure because authentication is
/// an infrastructure concern; the domain refers to users only by their string id.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    /// <summary>Key of the chosen avatar from the app's built-in collection.</summary>
    public string? AvatarId { get; set; }
}
