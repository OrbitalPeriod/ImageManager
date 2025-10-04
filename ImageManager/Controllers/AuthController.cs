using ImageManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(UserManager<User> userManager, SignInManager<User> signInManager) : Controller
{
    public record RegisterRequest(string Email, string Password);
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        return Ok(result.Succeeded ? "User created!" : string.Join(", ", result.Errors.Select(e => e.Description)));
    }
    
    public record LoginRequest(string Email, string Password);
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, true, false);
        return Ok(result.Succeeded ? "Login success" : "Login failed");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out");
    }
}