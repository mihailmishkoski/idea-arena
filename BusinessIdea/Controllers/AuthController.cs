using System.Security.Claims;
using BusinessIdea.Application.Common.Interfaces;
using BusinessIdea.Application.Common.Models;
using BusinessIdea.Infrastructure.Identity;
using BusinessIdea.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

/// <summary>
/// Cookie-based auth endpoints for the SPA. Registration issues an emailed
/// verification code; only confirm-email signs the new user in. Login and
/// confirmation share Identity's lockout counter against brute force.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    /// <summary>Token purpose; makes registration codes unusable anywhere else.</summary>
    private const string EmailConfirmationPurpose = "email-confirmation";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _email;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender email,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _email = email;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegistrationResultDto>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            AvatarId = request.AvatarId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        await SendVerificationCodeAsync(user);
        return Ok(new RegistrationResultDto(RequiresConfirmation: true, Email: user.Email!));
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> ConfirmEmail(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // One generic error for "no such user", "already confirmed" and "wrong
        // code" — the endpoint is anonymous, so it must not leak which is which.
        if (user is null || user.EmailConfirmed)
        {
            return InvalidCode();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Problem(
                title: "Too many attempts. Try again in a few minutes.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var valid = await _userManager.VerifyUserTokenAsync(
            user, TokenOptions.DefaultEmailProvider, EmailConfirmationPurpose, request.Code.Trim());
        if (!valid)
        {
            // Wrong codes count towards the same lockout as wrong passwords.
            await _userManager.AccessFailedAsync(user);
            return InvalidCode();
        }

        user.EmailConfirmed = true;

        // A new security stamp invalidates the just-used code (and any other
        // outstanding ones), so a captured code cannot be replayed. It also
        // persists the EmailConfirmed change.
        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.ResetAccessFailedCountAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User {UserId} confirmed their email.", user.Id);

        return Ok(ToDto(user));
    }

    [HttpPost("resend-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendCode(ResendCodeRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always report success so the endpoint cannot be used to probe which
        // emails have (unconfirmed) accounts.
        if (user is not null && !user.EmailConfirmed)
        {
            await SendVerificationCodeAsync(user);
        }

        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            return Ok(ToDto(user!));
        }

        if (result.IsNotAllowed)
        {
            // Correct password but unconfirmed email — the SPA routes to the
            // verification screen. 403 (not 401) is the discriminator.
            return Problem(
                title: "Email not confirmed.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        if (result.IsLockedOut)
        {
            return Problem(
                title: "Too many failed attempts. Try again in a few minutes.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Problem(
            title: "Invalid email or password.",
            statusCode: StatusCodes.Status401Unauthorized);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(id!);
        return user is null ? Unauthorized() : Ok(ToDto(user));
    }

    [HttpPut("avatar")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateAvatar(UpdateAvatarRequest request)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(id!);
        if (user is null)
        {
            return Unauthorized();
        }

        user.AvatarId = request.AvatarId;
        await _userManager.UpdateAsync(user);
        return Ok(ToDto(user));
    }

    private async Task SendVerificationCodeAsync(ApplicationUser user)
    {
        var code = await _userManager.GenerateUserTokenAsync(
            user, TokenOptions.DefaultEmailProvider, EmailConfirmationPurpose);

        var body =
            $"<p>Hi {user.DisplayName},</p>" +
            "<p>Your Idea Arena verification code is:</p>" +
            $"<p style=\"font-size:28px;font-weight:bold;letter-spacing:6px\">{code}</p>" +
            "<p>Enter it on the verification screen to finish creating your account. " +
            "The code expires in about 10 minutes.</p>" +
            "<p>If you did not sign up for Idea Arena, you can ignore this email.</p>";

        try
        {
            await _email.SendAsync(
                new EmailMessage(user.Email!, user.DisplayName,
                    "Your Idea Arena verification code", body),
                HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            // The account exists and the code can be re-requested from the
            // verification screen — a delivery hiccup must not fail registration.
            _logger.LogError(ex, "Failed to send verification code to user {UserId}.", user.Id);
        }
    }

    private ObjectResult InvalidCode() =>
        Problem(title: "Invalid or expired code.", statusCode: StatusCodes.Status400BadRequest);

    private static UserDto ToDto(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.AvatarId);
}
