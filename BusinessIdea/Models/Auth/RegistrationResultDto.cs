namespace BusinessIdea.Web.Models.Auth;

/// <summary>
/// Registration no longer signs the user in; the SPA must collect the emailed
/// verification code and call confirm-email to finish.
/// </summary>
public record RegistrationResultDto(bool RequiresConfirmation, string Email);
