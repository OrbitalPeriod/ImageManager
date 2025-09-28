using ImageManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace ImageManager;

[Route("test-auth")]
public class TestAuthController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public TestAuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("register")]
    public async Task<IActionResult> Register()
    {
        var user = new User { UserName = "test@example.com", Email = "test@example.com" };
        var result = await _userManager.CreateAsync(user, "Password123!");

        return Ok(result.Succeeded ? "User created!" : string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        var result = await _signInManager.PasswordSignInAsync("test@example.com", "Password123!", true, false);
        return Ok(result.Succeeded ? "Login success" : "Login failed");
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Not logged in");
    }

    [Authorize]
    [HttpGet("secret")]
    public IActionResult Secret()
    {
        return Ok("You are logged in!");
    }
}