using System.ComponentModel.DataAnnotations;

namespace BusinessIdea.Web.Models.Auth;

/// <summary>Body for changing the current user's avatar.</summary>
public record UpdateAvatarRequest([Required, StringLength(32)] string AvatarId);
