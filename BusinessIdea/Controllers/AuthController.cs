using System.Security.Claims;
using BusinessIdea.Infrastructure.Identity;
using BusinessIdea.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIdea.Web.Controllers;

/// <summary>
/// Cookie-based auth endpoints for the SPA. Registration and login issue the
/// same Identity cookie the rest of the API authenticates against, so the
/// Angular client just needs to send credentials on its requests.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
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

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Ok(ToDto(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid email or password."
            });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        return Ok(ToDto(user!));
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

    private static UserDto ToDto(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.AvatarId);
}
