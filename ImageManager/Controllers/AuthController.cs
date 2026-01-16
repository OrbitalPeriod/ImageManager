#region Usings
using System.ComponentModel.DataAnnotations;
using ImageManager.Data.Models;
using ImageManager.Services.FolderImport;
using Microsoft.AspNetCore.Authorization;
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
                           IFolderImportService folderImportService,
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

    /// <summary>
    /// Payload for changing password.
    /// </summary>
    public record ChangePasswordRequest(
        [Required, MinLength(6)] string CurrentPassword,
        [Required, MinLength(6)] string NewPassword);
    #endregion

    #region Actions
    /// <summary>
    /// Creates a new user account with the supplied email and password.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorsResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var user = new User 
        { 
            UserName = request.Username, 
            Email = request.Email,
            IsApproved = false
        };
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User '{Email}' registered successfully.", request.Email);
            
            // Create import folder for the new user
            try
            {
                folderImportService.CreateUserFolder(user.Id);
            }
            catch (Exception ex)
            {
                // Log but don't fail user creation if folder creation fails
                logger.LogWarning(ex, "Failed to create import folder for user {UserId} during registration", user.Id);
            }
            
            return Ok(new RegisterResponse("Registration successful. Your account is pending administrator approval."));
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        
        // Check if user exists and credentials are valid
        if (user != null)
        {
            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (passwordValid)
            {
                // Check if user is approved
                if (!user.IsApproved)
                {
                    logger.LogWarning("Login attempt by unapproved user '{Email}'.", request.Username);
                    return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Your account is pending administrator approval."));
                }

                // User is approved, proceed with sign in
                var signInResult = await signInManager.PasswordSignInAsync(
                    request.Username,
                    request.Password,
                    isPersistent: false,          // No persistent cookie for API
                    lockoutOnFailure: false);

                if (signInResult.Succeeded)
                {
                    logger.LogInformation("User '{Email}' logged in successfully.", request.Username);
                    return Ok(new { Message = "Login success" });
                }
            }
        }

        logger.LogWarning(
            "Login failed for {Email}.",
            request.Username);

        return Unauthorized(new ErrorResponse("Login failed"));
    }

    /// <summary>
    /// Signs the current user out.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
        return Ok(new { Message = "Logged out" });
    }

    /// <summary>
    /// Changes the password for the current authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse("User not found"));
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (changePasswordResult.Succeeded)
        {
            logger.LogInformation("User '{UserId}' changed their password successfully.", user.Id);
            return Ok(new { Message = "Password changed successfully" });
        }

        // Log the failure and return a structured error response.
        var errors = changePasswordResult.Errors.Select(e => e.Description).ToList();
        logger.LogWarning(
            "Password change failed for user '{UserId}'. Errors: {@Errors}",
            user.Id,
            errors);

        return BadRequest(new ErrorsResponse(errors));
    }
    #endregion
}
