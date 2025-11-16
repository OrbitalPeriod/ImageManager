#region Usings
using System.ComponentModel.DataAnnotations;
using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace ImageManager.Controllers;

/// <summary>
/// Handles authentication (register, login, logout) for the API.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<User> userManager,
                           SignInManager<User> signInManager,
                           ILogger<AuthController> logger) : ControllerBase
{
    #region DTOs
    /// <summary>
    /// Payload for registering a new user.
    /// </summary>
    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(6)] string Password,
        [Required, MinLength(6)] string Username);

    /// <summary>
    /// Response returned when registration succeeds.
    /// </summary>
    public record RegisterResponse(string Message);

    /// <summary>
    /// Payload for logging in.
    /// </summary>
    public record LoginRequest(
        [Required] string Username,
        [Required, MinLength(6)] string Password);

    /// <summary>Simple error wrapper.</summary>
    public record ErrorResponse(string Message);

    /// <summary>Container for multiple validation errors.</summary>
    public record ErrorsResponse(IEnumerable<string> Errors);
    #endregion

    #region Actions
    /// <summary>
    /// Creates a new user account with the supplied email and password.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var user = new User { UserName = request.Username, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User '{Email}' registered successfully.", request.Email);
            return Ok(new RegisterResponse("User created"));
        }

        // Log the failure and return a structured error response.
        var errors = result.Errors.Select(e => e.Description).ToList();
        logger.LogWarning(
            "Failed registration attempt for {Email}. Errors: {@Errors}",
            request.Email,
            errors);

        return BadRequest(new ErrorsResponse(errors));
    }

    /// <summary>
    /// Authenticates a user and issues an authentication cookie/session.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Username,
            request.Password,
            isPersistent: false,          // No persistent cookie for API
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            logger.LogInformation("User '{Email}' logged in successfully.", request.Username);
            return Ok(new { Message = "Login success" });
        }

        logger.LogWarning(
            "Login failed for {Email}. Result: {@Result}",
            request.Username,
            result);

        return Unauthorized(new ErrorResponse("Login failed"));
    }

    /// <summary>
    /// Signs the current user out.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        return Ok(new { Message = "Logged out" });
    }
    #endregion
}
