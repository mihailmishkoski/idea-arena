namespace BusinessIdea.Web.Models.Auth;

/// <summary>The authenticated user's public profile, returned to the SPA.</summary>
public record UserDto(string Id, string Email, string? DisplayName, string? AvatarId);
